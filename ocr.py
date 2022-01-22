"""
Skripta moze da se pokrene sa sledećim argumentima:
1. (obavezan) Put do PDF fajla nad kome treba uraditi OCR.
2. (opcioni) Put do TXT fajla u koji treba da se upise rezultat OCR-a.
    Ako ovaj argument nije naveden, rezultat će biti ispisan na stadardni output,
    tako da rezultat moze da se dobije u C# programu umesto da se odmah upise u
    fajl ukoliko ima potrebe za tim.

Iz C# se moze pokrenuti sa argumentima uz pomoć ProcessStartInfo.Arguments.
"""

from pdf2image import convert_from_path
# tesseract must be installed systemwide aswell
# pip install pytesseract
from pytesseract import image_to_string
import cv2  # pip install opencv-python
import sys
import os


TMP_IMG = 'tmp.jpg'


def convert_image_to_text(file):
    img = cv2.imread(file)
    img = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
    text = image_to_string(img, config='--psm 6').strip()

    return text


def get_text_from_any_pdf(pdf_file):
    print("Performing OCR, please wait...")
    images = convert_from_path(pdf_file)
    final_text = ""

    for pg, img in enumerate(images):
        img.save(TMP_IMG, 'JPEG')
        final_text += convert_image_to_text(TMP_IMG)
    os.remove(TMP_IMG)

    return final_text


def main():
    if len(sys.argv) < 2:
        sys.exit(1)  # nije prosledjen ni jedan argument

    path_to_pdf = sys.argv[1]
    path_to_txt = None
    if len(sys.argv) > 2:
        path_to_txt = sys.argv[2]

    ocr_text = get_text_from_any_pdf(path_to_pdf)

    if path_to_txt is not None:
        with open(path_to_txt, "w") as text_file:
            text_file.write(ocr_text)
    else:
        print(ocr_text)


if __name__ == '__main__':
    main()
