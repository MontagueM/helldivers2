import os
import shutil

test_folder = r"C:\Users\monta\OneDrive\helldivers2\saved\textures"
os.makedirs(os.path.join(test_folder, 'landscape'), exist_ok=True)
for file in os.listdir(test_folder):
    # get dimensions of file
    if not file.endswith('.dds'):
        continue

    f = open(os.path.join(test_folder, file), 'rb')
    f.seek(0x0C)
    height = int.from_bytes(f.read(4), byteorder='little')
    width = int.from_bytes(f.read(4), byteorder='little')

    if width > height and width > 64:
        print(f"{file} is landscape")
        # copy file into landscape folder
        shutil.copy(os.path.join(test_folder, file), os.path.join(test_folder, 'landscape', file))