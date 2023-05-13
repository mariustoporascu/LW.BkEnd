import logging
#import cv2
#import numpy as np
#from pyzbar import pyzbar
from PIL import Image
import zxing
import io

import azure.functions as func

def decode_barcodes_qrcodes(image):
    #barcodes = pyzbar.decode(image)
    reader = zxing.BarCodeReader()
    print(reader.zxing_version, reader.zxing_version_info)
    barcodes = reader.decode(image)
    print(f'Barcodes: {barcodes}')
    decoded_objects = []

    if isinstance(barcodes, zxing.BarCode):
        decoded_objects.append({
            "data": barcodes.raw,
        })
    else:
        decoded_objects.append({
            "data": barcodes[0].raw,
        })

    return decoded_objects

def main(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Python HTTP trigger function processed a request.')

    try:
        img_data = req.get_body()
        #np_arr = np.frombuffer(img_data, np.uint8)
        img_buffer = io.BytesIO(img_data)
        image = Image.open(img_buffer)#cv2.imdecode(np_arr, cv2.IMREAD_ANYCOLOR)

        decoded_objects = decode_barcodes_qrcodes(image)

        if decoded_objects[0]['data'] is not None:
            return func.HttpResponse(f"{decoded_objects[0]}")
        else:
            return func.HttpResponse(status_code=204)

    except Exception as e:
        logging.error(f"Error processing image: {str(e)}")
        return func.HttpResponse(status_code=400)
