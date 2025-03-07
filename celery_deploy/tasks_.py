from time import sleep
from celery import Celery
from celery.schedules import crontab
from Database.db import Base, User, File, UserFile, BlobFile
from Dto.FileFromCacheDto import FileFromCacheDto, UsersEmailsFileNames, PersonalisedInfoDto, FileFromCacheDto_v2
from Dto.FileTransferDto import *
from Dto.RsaKeyFileKeyDto import *
from services.ApiCallsService import ApiCall
from services.AuthenticateService import ProxyClass
import base64
import os
from dotenv import load_dotenv

load_dotenv()

celery = Celery("tasks", broker=os.environ.get("REDIS_URL"), backend=os.environ.get("REDIS_URL"),include=["tasks_"])
# celery = Celery("tasks", broker="redis://redis:6379/0", backend="redis://redis:6379/0",include=["tasks_"])
# celery.conf.update(
#     broker_connection_retry_on_startup=True,
# )

print('here')

# try:
#     celery = Celery("tasks", broker="http://redis:6379/0", backend="http://redis:6379/0",include=["tasks_"])
#     print('here')
# except Exception as e:
#     print('exceptie')
#     print(f"Error: {str(e)}")
# print('here')

'''
@celery.task()
def sendFilesToServer():
    try:
        gateWay = ApiCall(os.environ.get("backendBaseUrl"))
        pc = ProxyClass()
        pc.getProxyTokenSYNC()
        basedb = Base()
        session = basedb.getSession()
        print('---------------------------------------')
        blobs = session.query(BlobFile).all()
        if len(blobs) == 0:
            return
        
        for f in blobs:            
            base64EncFile = base64.b64encode(f.base64EncFile).decode('utf-8')
            base64Tag = f.file[0].tag
            user_files = session.query(UserFile).filter(UserFile.file_id == f.file[0].id).all()
            personalisedInfo = []
            for uf in user_files:
                user = session.query(User).filter(User.id == uf.user_id).first()
                personalisedInfo.append(PersonalisedInfoDto(fileName=uf.fileName, base64key=uf.key, base64iv=uf.iv, email=user.email, UploadDate=str(uf.upload_date)))
            filesMetaDto = FileFromCacheDto_v2(base64EncFile, base64Tag=base64Tag, personalisedList=personalisedInfo)
            response = gateWay.callBackendPostMethodDtoSYNC("/api/File/saveFileFromCache", pc.token, filesMetaDto)
            
            if response.status_code != 200:
                raise Exception(response.text)
            
        
        files = session.query(File).all()
        blob_files = session.query(BlobFile).all()
        user_files = session.query(UserFile).all()

        for user_file in user_files:
            session.delete(user_file)

        for file in files:
            session.delete(file)

        for blob_file in blob_files:
            session.delete(blob_file)
            
        session.commit()

    except Exception as e:
        print(f"Error fetching files and dates: {str(e)}")
    finally:
        session.close()
        print('finally')

'''
    
@celery.task()
def sendFilesToServer_v2():
    try:
        print('begining')
        gateWay = ApiCall(os.environ.get("backendBaseUrl"))
        print(gateWay.api_url)
        print('after gateway')
        pc = ProxyClass()
        print('after proxy class')
        pc.getProxyTokenSYNC()
        # await pc.getProxyToken()
        print('afster proxy sync')
        basedb = Base()
        print('here')
        session = basedb.getSession()
        print('after here')
        print('---------------------------------------')
        blobs = session.query(BlobFile).all()
        if len(blobs) == 0:
            return
        personalisedInfo = []
        for f in blobs:            
            file_path = f.encFilePath
            base64Tag = f.file[0].tag
            fileSize = f.file[0].size
            user_files = session.query(UserFile).filter(UserFile.file_id == f.file[0].id).all()
            personalisedInfo = []
            for uf in user_files:
                user = session.query(User).filter(User.id == uf.user_id).first()
                personalisedInfo.append(PersonalisedInfoDto(fileName=uf.fileName, base64key=uf.key, base64iv=uf.iv, email=user.email, UploadDate=str(uf.upload_date)))
            filesMetaDto = FileFromCacheDto_v2(fileSize=fileSize,encFilePath=file_path, base64Tag=base64Tag, personalisedList=personalisedInfo)
            response = gateWay.sendChunkFromFile("/api/File/writeFileOnDisk", "/api/File/saveFileFromCacheParams",pc.token, f.encFilePath, filesMetaDto)
            
            if response.status_code != 200:
                raise Exception(response.text)
                
        files = session.query(File).all()
        blob_files = session.query(BlobFile).all()
        
        for blob_f  in blob_files:
            if os.path.exists(blob_f.encFilePath):
                os.remove(blob_f.encFilePath)
        
        user_files = session.query(UserFile).all()

        for user_file in user_files:
            session.delete(user_file)

        for file in files:
            session.delete(file)

        for blob_file in blob_files:
            session.delete(blob_file)
            
        session.commit()

    except Exception as e:
        print(f"Error fetching files and dates: {str(e)}")
        raise Exception("Not good")
    finally:
        session.close()
        print('finally')

# @celery.task()
# def transferFileBetweenUsers(senderEmail:str ,senderToken:str,recieverEmail:str,  fileName:str, base64EncKey:str, base64EncIv:str):
#     gateWay = ApiCall(os.environ.get("backendBaseUrl"))
#     pc = ProxyClass()
#     pc.getProxyTokenSYNC()
#     basedb = Base()
#     session = basedb.getSession()
#     print('---------------------------------------')
#     userFile = session.query(UserFile).join(User, User.id == UserFile.user_id).filter(UserFile.fileName == fileName, User.email == senderEmail).first()
#     file = session.query(File).filter(File.id == userFile.file_id).first()
#     blob_file_id = file.blob_file_id
#     blobFile = session.query(BlobFile).filter(BlobFile.id == file.blob_file_id).first()
    
#     base64EncFile = base64.b64encode(blobFile.base64EncFile).decode('utf-8')
#     base64Tag = file.tag
#     user_files = session.query(UserFile).filter(UserFile.file_id == file.id).all()
#     personalisedInfo = []
    
#     for uf in user_files:
#         user = session.query(User).filter(User.id == uf.user_id).first()
#         personalisedInfo.append(PersonalisedInfoDto(fileName=uf.fileName, base64key=uf.key, base64iv=uf.iv, email=user.email, UploadDate=str(uf.upload_date)))
        
#     filesMetaDto = FileFromCacheDto(base64EncFile=base64EncFile, base64Tag=base64Tag, personalisedList=personalisedInfo)
#     response = gateWay.callBackendPostMethodDtoSYNC("/api/File/saveFileFromCache", pc.token, filesMetaDto)
#     if response.status_code != 200:
#         raise Exception(response.text)
    
#     file = session.query(File).filter(File.blob_file_id == blob_file_id).all()
#     blobFile = session.query(BlobFile).filter(BlobFile.id == file[0].blob_file_id).first()
#     user_files = session.query(UserFile).filter(UserFile.file_id == file[0].id).all()
    
#     for user_file in user_files:
#         session.delete(user_file)

#     for f in file:
#         session.delete(f)

#     session.delete(blobFile)
        
#     session.commit()
    
#     response = gateWay.callBackendPostMethodDtoSYNC("/api/File/sendFile", pc.token, FileTransferDto(senderToken=senderToken, recieverEmail=recieverEmail, fileName=fileName, base64EncKey=base64EncKey, base64EncIv=base64EncIv))
#     if response.status_code != 200:
#         raise Exception(response.text)
    
@celery.task()
def test_task():
    print("test task")
    return "test task"