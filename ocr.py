from pdf2image import convert_from_path
from pytesseract import image_to_string     # pytesseract must be installed systemwide aswell
import cv2      # pip install opencv-python
import os

imgPathTemp = 'out.jpg'

def save_txt(text, fileName):
    text_file = open(fileName, "w")
    text_file.write(text)
    text_file.close()

def convert_pdf_to_img(pdf_file):
    return convert_from_path(pdf_file)

def convert_image_to_text(file):
    img = cv2.imread(file)
    img = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
    text = image_to_string(img, config='--psm 6').strip()
    return text

def get_text_from_any_pdf(pdf_file):
    print("Performing OCR, please wait...")
    images = convert_pdf_to_img(pdf_file)
    final_text = ""
    for pg, img in enumerate(images):
        img.save(imgPathTemp, 'JPEG')
        final_text += convert_image_to_text(imgPathTemp)
    return final_text

path_to_pdf = 'file.pdf'
ocrText = get_text_from_any_pdf(path_to_pdf)
save_txt(ocrText, "text_ocr-{}".format(path_to_pdf))
os.remove(imgPathTemp)
