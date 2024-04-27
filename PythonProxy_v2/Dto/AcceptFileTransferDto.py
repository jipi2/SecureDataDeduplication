from pydantic import BaseModel

class AcceptFileTransferDto(BaseModel):
    senderEmail:str
    receiverEmail:str
    fileName:str
    base64FileKey:str
    base64FileIv:str