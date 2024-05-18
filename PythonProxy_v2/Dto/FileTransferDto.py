from pydantic import BaseModel
class FileTransferDto(BaseModel):
    senderToken: str
    recieverEmail: str
    fileName: str
    fullPath:str
    base64EncKey: str
    base64EncIv: str
    base64Tag:str
    isInCache: bool