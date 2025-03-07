from pydantic import BaseModel
from datetime import datetime
from typing import List

class FilesNameDate(BaseModel):
    fileName: str
    fileSize: float
    uploadDate: datetime