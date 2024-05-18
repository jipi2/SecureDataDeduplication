from pydantic import BaseModel

class AcceptFileTransferDto(BaseModel):
    senderEmail:str
    receiverEmail:str
    fileName:str
    fullPath:str
    base64FileKey:str
    base64FileIv:str
    base64Tag:str