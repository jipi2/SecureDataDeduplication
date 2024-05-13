
from dotenv import load_dotenv
from azure.storage.blob import BlobServiceClient
import os

# def download_blob(blob_name: str, file_path: str):
#     load_dotenv()
#     try:
#         conn_str = os.environ.get('azure_connection_string')
#         container_name = os.environ.get('azure_container_name')
        
#         blob_service_client = BlobServiceClient.from_connection_string(conn_str)
#         blob_client = blob_service_client.get_blob_client(container=container_name, blob=blob_name)
        
#         with open(file_path, "wb") as download_file:
#             download_file.write(blob_client.download_blob().readall())

#         print(f"Blob downloaded to {file_path} successfully.")
#     except Exception as e:
#         print(str(e))
#         raise e

async def download_blob(blob_name: str):
    try:
        print('here')
        conn_str = os.environ.get('azure_connection_string')
        container_name = os.environ.get('azure_container_name')
        
        blob_service_client = BlobServiceClient.from_connection_string(conn_str, max_chunk_get_size=1024*1024*20)
        blob_client = blob_service_client.get_blob_client(container=container_name, blob=blob_name)    
        # Stream the blob data directly to the client
        stream = blob_client.download_blob(max_concurrency=6)
        for chunk in stream.chunks():
            yield chunk
    except Exception as e:
        print(str(e))
        raise e