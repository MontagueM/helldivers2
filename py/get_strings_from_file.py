# find all null-terminated strings

file = r"D:\SteamLibrary\steamapps\common\Helldivers 2\data\000d250a449ec1e8"

with open(file, 'rb') as f:
    data = f.read()

cache = ''
for i in range(len(data)):
    # keep a cache of valid characters until we find a null byte, or reset if we find a non-valid character
    if chr(data[i]) in 'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_-/\\.':
        cache += chr(data[i])
    else:
        if len(cache) > 5 and '/' in cache:
            print(cache)
        cache = ''