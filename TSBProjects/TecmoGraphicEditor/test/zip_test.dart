import 'dart:convert';
import 'dart:typed_data';

import 'package:tecmo_graphic_editor/zip.dart';
import 'package:test/test.dart';

int u16(Uint8List b, int at) => b[at] | b[at + 1] << 8;
int u32(Uint8List b, int at) => u16(b, at) | u16(b, at + 2) << 16;

void main() {
  test('crc32 matches the standard check value', () {
    expect(crc32(ascii.encode('123456789')), 0xCBF43926);
  });

  test('stores files with headers a zip reader can follow', () {
    final hello = Uint8List.fromList(ascii.encode('hello'));
    final zip = storedZip({'a.txt': hello, 'bills-logo.png': Uint8List.fromList([1, 2, 3])});

    // End of central directory: 2 entries, directory offset/size point inside the file.
    final end = zip.length - 22;
    expect(u32(zip, end), 0x06054B50);
    expect(u16(zip, end + 10), 2);
    final dirSize = u32(zip, end + 12), dirAt = u32(zip, end + 16);
    expect(dirAt + dirSize, end);

    // First central entry -> its local header -> its data.
    expect(u32(zip, dirAt), 0x02014B50);
    expect(u32(zip, dirAt + 16), crc32(hello));
    final nameLen = u16(zip, dirAt + 28);
    expect(ascii.decode(zip.sublist(dirAt + 46, dirAt + 46 + nameLen)), 'a.txt');
    final local = u32(zip, dirAt + 42);
    expect(u32(zip, local), 0x04034B50);
    expect(u16(zip, local + 8), 0); // stored
    final dataAt = local + 30 + u16(zip, local + 26) + u16(zip, local + 28);
    expect(zip.sublist(dataAt, dataAt + 5), hello);
  });
}
