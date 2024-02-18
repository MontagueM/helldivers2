import xxhash
import os

source_file = r"C:\Users\monta\OneDrive\helldivers2\saved\textures\009da023c64d178d_27.dds"
x = xxhash.xxh32()
with open(source_file, 'rb') as f:
    x.update(f.read())

print(x.hexdigest())

test_folder = r"C:\Users\monta\OneDrive\helldivers2\saved\textures"
match_count = 0
for file in os.listdir(test_folder):
    # make sure not same file as source
    if file == os.path.basename(source_file):
        print(f"Skipping {file}")
        continue
    if file.endswith('.dds'):
        y = xxhash.xxh32()
        with open(os.path.join(test_folder, file), 'rb') as f:
            y.update(f.read())
        if x.digest() == y.digest():
            match_count += 1
            print(f"{file} matches ({match_count})")