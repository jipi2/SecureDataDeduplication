    
from pydantic import BaseModel

class TransferVerificationDto(BaseModel):
    senderEmail:str
    receiverEmail:str
    fileName:str