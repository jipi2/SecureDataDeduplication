from pydantic import BaseModel


class CapsuleDto(BaseModel):
    base64KeyCapsule:str
    base64IvCapsule:str
    fileName:str
    destEmail:str