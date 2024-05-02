from pydantic import BaseModel

class FileInfoFromCache(BaseModel):
    fileSize:float
    base64Tag:str
    fileName:str
    userEmail:str
    uploadDate:str