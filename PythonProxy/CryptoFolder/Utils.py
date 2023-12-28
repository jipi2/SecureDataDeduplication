import hashlib
from .MerkleTree import MerkleTree
from .MTMember import MTMember
import io
import jwt
import base64

def getUserId(token:str):
    decoded_token = jwt.decode(token, algorithms=["HS256"], options={"verify_signature": False})
    return decoded_token["UserId"]

def generateHashForTag(base64EncFile: str) -> str:
    bytesEncFile = base64.b64decode(base64EncFile)
    hash = hashlib.sha3_256(bytesEncFile).digest()
    tag = base64.b64encode(hash).decode()
    return tag

def get_leaves(MT: MerkleTree, count: int):
    try:
        while count > 1:
            count = 0
            level = MT._Levels + 1
            n = len(MT._HashTree)
            aux = MerkleTree()

            for i in range(MT._IndexOfLevel[MT._Levels], n, 2):
                h = hashlib.sha3_256(MT._HashTree[i]._hash + MT._HashTree[i + 1]._hash).digest()
                aux._HashTree.append(MTMember(level, h))
                count += 1

            MT._HashTree.extend(aux._HashTree)

            if count % 2 != 0 and count != 1:
                MT._HashTree.append(MT._HashTree[-1])
                count += 1

            MT._IndexOfLevel.append(len(MT._HashTree) - count)
            MT._Levels += 1

    except Exception as e:
        print(e)

def get_merkle_tree(file: io.BytesIO) -> MerkleTree:
    MT = MerkleTree()
    file_size = file.seek(0, 2)
    file.seek(0)

    buffer_size = 1024
    buffer = bytearray(buffer_size)

    count = 0
    while (bytes_read := file.readinto(buffer)) > 0:
        h = hashlib.sha3_256(buffer[:bytes_read]).digest()
        MT._HashTree.append(MTMember(0, h))
        count += 1

    if count % 2 != 0:
        MT._HashTree.append(MT._HashTree[0])
        count += 1

    MT._IndexOfLevel.append(0)
    get_leaves(MT, count)

    return MT



def verify_challenge(base64EncFile: str, base64TagEnc:str):
    print("")
    
    