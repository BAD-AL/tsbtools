// A minimal zip writer: files are stored uncompressed (PNGs are already
// compressed), which every zip tool can open.
import 'dart:convert';
import 'dart:typed_data';

final _crcTable = List<int>.generate(256, (n) {
  var c = n;
  for (var k = 0; k < 8; k++) {
    c = c & 1 != 0 ? 0xEDB88320 ^ (c >> 1) : c >> 1;
  }
  return c;
});

int crc32(List<int> data) {
  var c = 0xFFFFFFFF;
  for (final b in data) {
    c = _crcTable[(c ^ b) & 0xFF] ^ (c >> 8);
  }
  return c ^ 0xFFFFFFFF;
}

/// Builds a zip holding [files] (name -> bytes), in map order.
Uint8List storedZip(Map<String, Uint8List> files) {
  final out = BytesBuilder();
  final dir = BytesBuilder();
  void u16(BytesBuilder b, int v) => b.add([v & 0xFF, v >> 8 & 0xFF]);
  void u32(BytesBuilder b, int v) => b.add([v & 0xFF, v >> 8 & 0xFF, v >> 16 & 0xFF, v >> 24 & 0xFF]);

  for (final MapEntry(key: name, value: data) in files.entries) {
    final nameBytes = utf8.encode(name);
    final crc = crc32(data), at = out.length;
    // version, flags (bit 11 = UTF-8 name), method 0 = stored, time, date (1980-01-01)
    void common(BytesBuilder b) {
      u16(b, 20);
      u16(b, 0x0800);
      u16(b, 0);
      u16(b, 0);
      u16(b, 0x21);
      u32(b, crc);
      u32(b, data.length);
      u32(b, data.length);
      u16(b, nameBytes.length);
      u16(b, 0); // extra field length
    }

    u32(out, 0x04034B50);
    common(out);
    out
      ..add(nameBytes)
      ..add(data);

    u32(dir, 0x02014B50);
    u16(dir, 20); // made by
    common(dir);
    u16(dir, 0); // comment length
    u16(dir, 0); // disk
    u16(dir, 0); // internal attributes
    u32(dir, 0); // external attributes
    u32(dir, at);
    dir.add(nameBytes);
  }

  final dirAt = out.length, dirSize = dir.length;
  out.add(dir.takeBytes());
  u32(out, 0x06054B50);
  u16(out, 0);
  u16(out, 0);
  u16(out, files.length);
  u16(out, files.length);
  u32(out, dirSize);
  u32(out, dirAt);
  u16(out, 0);
  return out.takeBytes();
}
