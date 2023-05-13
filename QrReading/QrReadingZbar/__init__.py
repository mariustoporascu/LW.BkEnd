import logging
import cv2
import numpy as np
from pyzbar import pyzbar

import azure.functions as func

def decode_barcodes_qrcodes(image):
    barcodes = pyzbar.decode(image)
    decoded_objects = []
    print(f'Barcodes: {barcodes}')
    for barcode in barcodes:
        data = barcode.data.decode("utf-8")

        decoded_objects.append({
            "data": data
        })

    return decoded_objects

def main(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Python HTTP trigger function processed a request.')

    try:
        img_data = req.get_body()
        np_arr = np.frombuffer(img_data, np.uint8)
        image = cv2.imdecode(np_arr, cv2.IMREAD_ANYCOLOR)

        decoded_objects = decode_barcodes_qrcodes(image)

        if decoded_objects:
            return func.HttpResponse(f"{decoded_objects[0]}")
        else:
            return func.HttpResponse(status_code=204)

    except Exception as e:
        logging.error(f"Error processing image: {str(e)}")
        return func.HttpResponse(status_code=400)
