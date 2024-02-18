# find all null-terminated strings
import os
folder = r"D:\SteamLibrary\steamapps\common\Helldivers 2\data"

strings = {}
for file in os.listdir(folder):
    if '.' in file:
        continue

    print(f"--{file}--")
    with open(os.path.join(folder, file), 'rb') as f:
        data = f.read()

    cache = ''
    for i in range(len(data)):
        # keep a cache of valid characters until we find a null byte, or reset if we find a non-valid character
        if chr(data[i]) in 'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_-/\\.':
            cache += chr(data[i])
        else:
            if len(cache) > 9 and cache.startswith('content/'):
                print(cache)
                if cache not in strings:
                    strings[cache] = []
                strings[cache].append(file)
            cache = ''

with open('strings.txt', 'w') as f:
    for k, v in strings.items():
        f.write(f"--{k}--\n")
        for s in v:
            f.write(f"{s}\n")