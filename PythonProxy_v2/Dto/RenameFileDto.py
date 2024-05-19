from pydantic import BaseModel

class RenameFileDto(BaseModel):
    oldFullPath: str
    newFullPath: str
    isFolder: bool