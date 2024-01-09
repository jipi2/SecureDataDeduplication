from pydantic import BaseModel

class FileDedupDto(BaseModel):
    userEmail:str
    base64tag:str
    fileName:str