from pydantic import BaseModel

class FileInfoFromCache(BaseModel):
    base64Tag:str
    fileName:str
    userEmail:str
    uploadDate:str