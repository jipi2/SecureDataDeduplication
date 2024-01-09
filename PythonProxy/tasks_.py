from time import sleep
from celery import Celery
from celery.schedules import crontab
from Database.db import Base, User, File, user_file, BlobFile
from Dto.FileFromCacheDto import FileFromCacheDto, UsersEmailsFileNames
from services.ApiCallsService import ApiCall
from services.AuthenticateService import ProxyClass
import base64

celery = Celery("tasks", broker="redis://redis:6379/0", backend="redis://redis:6379/0",include=["tasks_"])

@celery.task()
def sendFilesToServer():
    try:
        gateWay = ApiCall("https://host.docker.internal:7109")
        pc = ProxyClass()
        pc.getProxyTokenSYNC()
        basedb = Base()
        session = basedb.getSession()
        blobs = session.query(BlobFile).all()
        if len(blobs) == 0:
            return
        
        for f in blobs:
            
            base64EncFile = base64.b64encode(f.base64EncFile).decode('utf-8')
            base64Tag = f.file[0].tag
            key = f.file[0].key
            iv = f.file[0].iv           
            files = session.query(File).filter(File.blob_file_id == f.id).all()
            emailsFilenames = []
            
            for file in files:
                users_and_uploadDates = session.query(user_file).filter(user_file.c.file_id == file.id).all()
                for user_and_uploadDate in users_and_uploadDates:
                    user = session.query(User).filter(User.id == user_and_uploadDate.user_id).first()
                    emailsFilenames.append(UsersEmailsFileNames(userEmail=user.email, fileName=file.fileName, uploadTime=str(user_and_uploadDate.upload_date)))
            
            filesMetaDto = FileFromCacheDto(base64EncFile=base64EncFile, base64Tag=base64Tag, key=key, iv=iv, emailsFilenames=emailsFilenames)
            response = gateWay.callBackendPostMethodDtoSYNC("/api/File/saveFileFromCache", pc.token, filesMetaDto)
            if response.status_code != 200:
                raise Exception(response.text)
        
        files = session.query(File).all()
        blob_files = session.query(BlobFile).all()

        for file in files:
            session.delete(file)

        for blob_file in blob_files:
            session.delete(blob_file)
            
        session.commit()

    except Exception as e:
        print(f"Error fetching files and dates: {str(e)}")
    finally:
        session.close()


@celery.task()
def test_task():
    print("test task")
    return "test task"