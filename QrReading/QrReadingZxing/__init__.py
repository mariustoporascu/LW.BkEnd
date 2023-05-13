import logging
from PIL import Image
import zxing
import io

import azure.functions as func

def decode_barcodes_qrcodes(image):
    reader = zxing.BarCodeReader()
    print(reader.zxing_version, reader.zxing_version_info)
    barcodes = reader.decode(image, True)
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
        img_buffer = io.BytesIO(img_data)
        image = Image.open(img_buffer)

        decoded_objects = decode_barcodes_qrcodes(image)

        if decoded_objects[0]['data'] is not None:
            return func.HttpResponse(f"{decoded_objects[0]}")
        else:
            return func.HttpResponse(status_code=204)

    except Exception as e:
        logging.error(f"Error processing image: {str(e)}")
        return func.HttpResponse(status_code=400)
