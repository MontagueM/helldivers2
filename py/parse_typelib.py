file = r"D:\SteamLibrary\steamapps\common\Helldivers 2\data\game\dl_library.dl_typelib"

with open(file, 'rb') as f:
    data = f.read()


string_offset = 788329
string_data = data[string_offset:]
strings = string_data.split(b'\x00')
for i, s in enumerate(strings):
    print(i, s.decode('utf-8'))
print(len(strings), hex(len(string_data)))
