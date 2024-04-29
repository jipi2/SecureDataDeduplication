from pydantic import BaseModel

class DeleteFileInfoDto(BaseModel):
    base64Tag:str
    fileName:str
    userEmail:str