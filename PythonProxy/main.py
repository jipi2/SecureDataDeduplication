from fastapi import FastAPI, Body, Depends, HTTPException
import httpx
from auth.auth_bearer import JWTBearer
from starlette.requests import Request
from decouple import config
from fastapi.middleware.cors import CORSMiddleware
from CryptoFolder.Utils import *

#SSL
# import ssl

#DTO-uri
from Dto.LoginUserDto import LoginUserDto
from  Dto.TestDto import TestDto
from Dto.Resp import Resp
from Dto.FIleParamsDto import FileParamsDto
from Dto.TagDto import TagDto

#Services
from services.FileService import FileService

base_url = config('backendBaseUrl')


app = FastAPI() 
#CORS
origins = [base_url]  # You can replace this with a list of allowed origins
app.add_middleware(
    CORSMiddleware,
    allow_origins=origins,
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

#SSL
# ssl_context = ssl.SSLContext(ssl.PROTOCOL_TLSv1_2)
# ssl_context.load_cert_chain('./SSL_Folder/fastApiServer.cer', './SSL_Folder/key_ca.prv')

@app.get("/")
def root():
    return {"message": "Hello World"}

@app.get("/testMethod")   
def testMethod():
    testDto = TestDto(id=1,name="test")
    return testDto

# @app.post("/testWithJwt")
# async def test_with_jwt(request: Request, loginDto:LoginUserDto):
#     authorization_header = request.headers.get("Authorization")

#     api_repsonse = await call_backend(base_url+'/api/User/Login',"", loginDto)
#     print(api_repsonse.json())
#     resp = Resp(Success=api_repsonse.json()["succes"], Message=api_repsonse.json()["message"], AccessToken=api_repsonse.json()["accessToken"])

#     return {"message": f"Hello World With Jwt, Token: ", "message": resp.Message}

@app.post("/uploadFile", tags = ['file'])
async def uploadFile(request: Request, fileParams:FileParamsDto ):
    authorization_header = request.headers.get("Authorization")
    token=""
    if authorization_header is not None:
        token = authorization_header.split(" ")[1]
    print(token)
    _fileService = FileService(token, fileParams)
    await _fileService.computeFileVerification()
    
    # tagDto = TagDto(encTag=fileParams.base64TagEnc)
    
    # reqTagResp = await call_backend(base_url+'/api/File/checkEncTag', token, tagDto)
    # if(reqTagResp.json() == True):
    #    # aici va urma schimbare de challenge-uri
    #    print("")
