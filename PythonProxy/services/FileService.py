from Dto.FIleParamsDto import FileParamsDto
from services.AuthenticateService import ProxyClass
from services.ApiCallsService import ApiCall
from CryptoFolder.Utils import *
from CryptoFolder.MerkleTree import MerkleTree
from Dto.FileResp import FileResp
from Dto.FileDecDto import FileDecDto
from Dto.FileEncDto import FileEncDto
from Dto.FileDedupDto import FileDedupDto
from Dto.ServerBlobFile import ServerBlobFile
from Dto.EncryptParamsDto import EncryptParamsDto
from Database.db import Base, User, File, user_file, BlobFile
from Dto.FilesNameDate import FilesNameDate
from Dto.EmailFilenameDto import EmailFilenameDto
import asyncio
from sqlalchemy.orm.exc import NoResultFound
import base64
import os
from dotenv import load_dotenv  

load_dotenv()


class FileService():
    def __init__(self, userToken:str, fileParams:FileParamsDto=None, filename:str=None):
        self.userToken = userToken
        self.userEmail = ""
        self.fileParams = fileParams
        self.filename = filename
        self.authService = ProxyClass()
        self.gateWay = ApiCall(os.environ.get("backendBaseUrl"))
    
    async def __getUserEmail(self):
        userEmail = await self.authService.getUserEmail(self.userToken)
        if(userEmail == None):
            print("e None")
            raise Exception("JWT not valid")
        self.userEmail = str(userEmail)
        
    def __getFileBase64Tag(self, base64EncFile:str):
        tag = generateHashForTag(base64EncFile)
        return tag
    
    async def __verifyTag(self, base64tag:str):
        response = await self.gateWay.callBackendPostMethodWithSimpleParams("/api/File/checkTag", self.authService.token, base64tag)
        if response.text == "false":
            return False
        return True
    
    async def __getMerkleTree(self):
        self.mt = get_merkle_tree(io.BytesIO(self.fileParams.base64EncFile.encode('utf-8')))
    
    async def __getChallenge(self, base64tag:str):
        response = await self.gateWay.callBackendPostMethodWithSimpleParams("/api/File/getChallenge",
                                                                            self.authService.token,
                                                                            base64tag)
        fileChallenge = FileMetaChallenge(**response.json())
        print(fileChallenge)
        return fileChallenge
    
    async def __getDecryptedFileParams(self):
        fileEncDto = FileEncDto(userEmail=self.userEmail,
                                base64KeyEnc=self.fileParams.base64KeyEnc,
                                base64IvEnc=self.fileParams.base64IvEnc,
                                encFileName=self.fileParams.encFileName)
        response = await self.gateWay.callBackendPostMethodDto("/api/File/getDecryptedFileParams",
                                                         self.authService.token,
                                                         fileEncDto)
        if response.status_code != 200:
            raise Exception(response.text)
        
        fileDecDto = FileDecDto(
            base64Key=response.json()['base64key'],
            base64Iv=response.json()['base64iv'],
            fileName=response.json()['fileName']
        )
        
        return fileDecDto
    
    async def __saveDeduplicateFileForUser(self, tag, fileName):
        fileDedupDto = FileDedupDto(userEmail=self.userEmail,base64tag=tag,
                                    fileName=fileName)
        response = await self.gateWay.callBackendPostMethodDto("/api/File/saveDeduplicateFileForUser",
                                                         self.authService.token,
                                                         fileDedupDto)
        if(response.status_code != 200):
            raise Exception(response.text)
    
    def __saveUser(self, session):
        new_user = User(email=self.userEmail)
        session.add(new_user)
        session.commit()
    
    def __saveFile(self, tag, fileDecDto, session):
        print('-----------------------------')
        print(self.fileParams.base64EncFile)
        print('-----------------------------')
        blob_file = BlobFile(base64EncFile=base64.b64decode(self.fileParams.base64EncFile))
        new_file = File(tag=tag, key=fileDecDto.base64Key, 
                        iv=fileDecDto.base64Iv, fileName=fileDecDto.fileName, 
                        blob_file=blob_file)
        session.add(new_file)
        session.commit()
    
    def __checkIfUserHasFile(self, user, file, session):
        result = session.query(user_file).filter(user_file.c.user_id==user.id,
                                                 user_file.c.file_id==file.id).first()
        if not result:
            return False
        return True
    
    async def __saveInCache(self, base64tag, fileDecDto):
        try:
            basedb = Base()
            session = basedb.getSession()
            user = session.query(User).filter(User.email==self.userEmail).first()
            if not user:
                self.__saveUser(session)
                user = session.query(User).filter(User.email==self.userEmail).first()

            file = session.query(File).filter(File.tag == base64tag).first()
            if not file:
                self.__saveFile(base64tag, fileDecDto, session)
                file = session.query(File).filter(File.tag == base64tag, File.fileName == fileDecDto.fileName).first()
            else:
                blob_file_id = file.blob_file_id
                file = session.query(File).filter(File.tag == base64tag, File.fileName == fileDecDto.fileName).first()
                if not file:
                    blobFile = session.query(BlobFile).filter(BlobFile.id == blob_file_id).first()
             
                    file = File(tag=base64tag, key=fileDecDto.base64Key, 
                            iv=fileDecDto.base64Iv, fileName=fileDecDto.fileName, 
                            blob_file=blobFile)
                    session.add(file)
                else:
                    if self.__checkIfUserHasFile(user, file, session) == True:
                        raise Exception("User already has this file")
                    
            user.files.append(file)
            session.commit()
            session.close()
            print('finally')
        finally:
            session.close()

    async def computeFileVerification(self):
        await self.authService.getProxyToken()
        await self.__getUserEmail()
        tag = self.__getFileBase64Tag(self.fileParams.base64EncFile)
        MakeMerkleTreetask = asyncio.create_task(self.__getMerkleTree())
        if await self.__verifyTag(tag) == False:
            print('save in cache')
            fileDecDto = await self.__getDecryptedFileParams()
            await self.__saveInCache(tag, fileDecDto)
        else:
            fileChallenge = await self.__getChallenge(tag)
            await MakeMerkleTreetask
            fileResp = getRespForchallenge(self.mt, fileChallenge, self.fileParams.base64EncFile)
            response = await self.gateWay.callBackendPostMethodDto("/api/File/verifyFileChallengeForProxy", self.authService.token, fileResp)
            if(response == False):
                raise Exception("File not verified")
            fileDecDto = await self.__getDecryptedFileParams()
            await self.__saveDeduplicateFileForUser(tag, fileDecDto.fileName)

    async def __getFileNameAndDatesFromCache(self):
        try:
            basedb = Base()
            session = basedb.getSession()
            query=(
                session.query(User.email, File.fileName, user_file.c.upload_date) #.c = columns
                .join(user_file, User.id == user_file.c.user_id)
                .join(File, user_file.c.file_id == File.id)
                .filter(User.email == self.userEmail)
            )
            result = query.all()
            files_and_dates = [(row.fileName, row.upload_date) for row in result]
            return files_and_dates
            
        except Exception as e:
            print(f"Error fetching files and dates: {e}")
            raise Exception("It was a problem, try later!")

    async def getFilesNamesAndDates(self):
        await self.authService.getProxyToken()
        await self.__getUserEmail()
        FileNameAndDateFromCacheTask = asyncio.create_task(self.__getFileNameAndDatesFromCache())
        response = await self.gateWay.callBackendPostMethodWithSimpleParams("/api/File/getUploadedFileNamesAndDatesWithoutProxy",
                                                     self.authService.token,
                                                     self.userEmail)
        
        if(response.status_code != 200):
            raise Exception(response.text)
        filesList = [FilesNameDate(**item) for item in response.json()]
        filesAndDatesFromCache = await FileNameAndDateFromCacheTask
        filesList.extend([FilesNameDate(fileName=file_name, uploadDate=upload_date) for file_name, upload_date in filesAndDatesFromCache])
        return filesList
    
    async def getFileFromCache(self, filename:str):
        try:
            basedb = Base()
            session = basedb.getSession()
            result = session.query(File.iv, File.key, BlobFile.base64EncFile).join(user_file, File.id == user_file.c.file_id).join(User, User.id == user_file.c.user_id).join(BlobFile, BlobFile.id == File.blob_file_id).filter(File.fileName == filename,User.email == self.userEmail).first()
            if result == None:
                return None
            else:
                return result
        except Exception as e:
            raise e
    
    async def getFileFromStorage(self):
        await self.authService.getProxyToken()
        try:
            await self.__getUserEmail()
            
            fileFromCache = await self.getFileFromCache(self.filename)
            if fileFromCache == None:
                emailFilename = EmailFilenameDto(userEmail=self.userEmail, fileName=self.filename)
                response = await self.gateWay.callBackendPostMethodDto("/api/File/proxyGetFileFromStorage", self.authService.token, emailFilename)
                return ServerBlobFile(FileName=response.json()["fileName"], FileKey=response.json()["fileKey"], EncBase64File=response.json()["encBase64File"], FileIv=response.json()["fileIv"])
            else:
                response = await self.gateWay.callBackendPostMethodDto("/api/File/encryptFileParamsForSendingToUser", self.authService.token, EncryptParamsDto(userEmail=self.userEmail, fileName=self.filename, fileKey=fileFromCache.key, fileIv=fileFromCache.iv))
                encParamsDto = EncryptParamsDto(**response.json())        
                return ServerBlobFile(FileName=encParamsDto.fileName, FileKey=encParamsDto.fileKey, EncBase64File=base64.b64encode(fileFromCache.base64EncFile).decode('utf-8'), FileIv=encParamsDto.fileIv)
        except Exception as e:
            raise e
        