from pydantic import BaseModel

class FileKeyAndIvDto(BaseModel):
    base64key:str
    base64iv:str
    base64Tag:str