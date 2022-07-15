from typing import List
from pkg_resources import parse_version
import kaitaistruct
from kaitaistruct import KaitaiStruct, KaitaiStream, BytesIO
import struct
from enum import Enum
import json

if parse_version(kaitaistruct.__version__) < parse_version('0.9'):
    raise Exception("Incompatible Kaitai Struct Python API: 0.9 or later is required, but you have %s" % (kaitaistruct.__version__))

# Copy game files into this directory or change the paths below
headpack_bin_path: str = "RestoreMorphs/HEADPACK.BIN"
kldata_bin_path: str = "RestoreMorphs/KLDATA.BIN"

class Headpack(KaitaiStruct):
    def __init__(self, _io, _parent=None, _root=None):
        self._io = _io
        self._parent = _parent
        self._root = _root if _root else self
        self._read()

    def _read(self):
        self.bin_count = self._io.read_u4le()
        self.kldata_offset = self._io.read_u4le()
        self.bgmpack_offset = self._io.read_u4le()
        self.pptpack_offset = self._io.read_u4le()
        self.headpack_size = self._io.read_u4le()

    class Pointers(KaitaiStruct):
        def __init__(self, start_offset, _io, _parent=None, _root=None):
            self._io = _io
            self._parent = _parent
            self._root = _root if _root else self
            self.start_offset = start_offset
            self._read()

        def _read(self):
            self.archive_count = self._io.read_u4le()

        @property
        def archives(self):
            if hasattr(self, '_m_archives'):
                return self._m_archives if hasattr(self, '_m_archives') else None

            if self.archive_count != 5:
                _pos = self._io.pos()
                self._io.seek((self.start_offset + 4))
                self._m_archives = [None] * (self.archive_count)
                for i in range(self.archive_count):
                    self._m_archives[i] = Headpack.Archive(self._io, self, self._root)

                self._io.seek(_pos)

            return self._m_archives if hasattr(self, '_m_archives') else None

        @property
        def pal_archives(self):
            if hasattr(self, '_m_pal_archives'):
                return self._m_pal_archives if hasattr(self, '_m_pal_archives') else None

            if self.archive_count == 5:
                _pos = self._io.pos()
                self._io.seek(self.start_offset)
                self._m_pal_archives = Headpack.PalArchive(self._io, self, self._root)
                self._io.seek(_pos)

            return self._m_pal_archives if hasattr(self, '_m_pal_archives') else None


    class Archive(KaitaiStruct):
        def __init__(self, _io, _parent=None, _root=None):
            self._io = _io
            self._parent = _parent
            self._root = _root if _root else self
            self._read()

        def _read(self):
            self.sector_offset = self._io.read_u4le()
            self.sectors = self._io.read_u4le()

        @property
        def offset(self):
            if hasattr(self, '_m_offset'):
                return self._m_offset if hasattr(self, '_m_offset') else None

            self._m_offset = (self.sector_offset * 2048)
            return self._m_offset if hasattr(self, '_m_offset') else None

        @property
        def size(self):
            if hasattr(self, '_m_size'):
                return self._m_size if hasattr(self, '_m_size') else None

            self._m_size = (self.sectors * 2048)
            return self._m_size if hasattr(self, '_m_size') else None


    class PalArchiveList(KaitaiStruct):
        def __init__(self, start_offset, i, _io, _parent=None, _root=None):
            self._io = _io
            self._parent = _parent
            self._root = _root if _root else self
            self.start_offset = start_offset
            self.i = i
            self._read()

        def _read(self):
            pass

        @property
        def kldata(self):
            if hasattr(self, '_m_kldata'):
                return self._m_kldata if hasattr(self, '_m_kldata') else None

            _pos = self._io.pos()
            self._io.seek((self.start_offset + self._parent.archive_offsets[self.i]))
            self._m_kldata = Headpack.Pointers((self.start_offset + self._parent.archive_offsets[self.i]), self._io, self, self._root)
            self._io.seek(_pos)
            return self._m_kldata if hasattr(self, '_m_kldata') else None


    class PalArchive(KaitaiStruct):
        def __init__(self, _io, _parent=None, _root=None):
            self._io = _io
            self._parent = _parent
            self._root = _root if _root else self
            self._read()

        def _read(self):
            self.pal_kldata_count = self._io.read_u4le()
            self.archive_offsets = [None] * (self.pal_kldata_count)
            for i in range(self.pal_kldata_count):
                self.archive_offsets[i] = self._io.read_u4le()


        @property
        def kldata_list(self):
            if hasattr(self, '_m_kldata_list'):
                return self._m_kldata_list if hasattr(self, '_m_kldata_list') else None

            self._m_kldata_list = [None] * (self.pal_kldata_count)
            for i in range(self.pal_kldata_count):
                self._m_kldata_list[i] = Headpack.PalArchiveList(self._parent.start_offset, i, self._io, self, self._root)

            return self._m_kldata_list if hasattr(self, '_m_kldata_list') else None


    @property
    def kldata(self):
        if hasattr(self, '_m_kldata'):
            return self._m_kldata if hasattr(self, '_m_kldata') else None

        _pos = self._io.pos()
        self._io.seek(self.kldata_offset)
        self._m_kldata = Headpack.Pointers(self.kldata_offset, self._io, self, self._root)
        self._io.seek(_pos)
        return self._m_kldata if hasattr(self, '_m_kldata') else None

    @property
    def bgmpack(self):
        if hasattr(self, '_m_bgmpack'):
            return self._m_bgmpack if hasattr(self, '_m_bgmpack') else None

        _pos = self._io.pos()
        self._io.seek(self.bgmpack_offset)
        self._m_bgmpack = Headpack.Pointers(self.bgmpack_offset, self._io, self, self._root)
        self._io.seek(_pos)
        return self._m_bgmpack if hasattr(self, '_m_bgmpack') else None

    @property
    def pptpack(self):
        if hasattr(self, '_m_pptpack'):
            return self._m_pptpack if hasattr(self, '_m_pptpack') else None

        _pos = self._io.pos()
        self._io.seek(self.pptpack_offset)
        self._m_pptpack = Headpack.Pointers(self.pptpack_offset, self._io, self, self._root)
        self._io.seek(_pos)
        return self._m_pptpack if hasattr(self, '_m_pptpack') else None



class Klfa(KaitaiStruct):
    """- animation
      - translations
        - joint0
          - keyframes
          - translations for each keyframe
        - joint1
          - keyframes
          - translations for each keyframe
        - ...
      - rotations
        - joint0
          - keyframes
          - rotations for each keyframe
        - joint1
          - keyframes
          - rotations for each keyframe
        - ...
    """
    def __init__(self, _io, _parent=None, _root=None):
        self._io = _io
        self._parent = _parent
        self._root = _root if _root else self
        self._read()

    def _read(self):
        self.joint_count = self._io.read_u2le()
        self.keyframe_count = self._io.read_u2le()
        self.more_joint_counts = self._io.read_bytes(4)
        self.name = (KaitaiStream.bytes_terminate(self._io.read_bytes(8), 0, False)).decode(u"ASCII")
        self.st = self._io.read_bytes(4)
        self.morphs_offset = self._io.read_u4le()
        self.data_offset = self._io.read_u4le()
        self.more_stuff = self._io.read_bytes((((6 + self.data_offset) - 32) + 6))
        self.initial_pos = Klfa.FloatCoordinate(self._io, self, self._root)
        self.scale = self._io.read_f4le()
        self.joint_translations = [None] * (self.joint_count)
        for i in range(self.joint_count):
            self.joint_translations[i] = Klfa.JointTranslation(self._io, self, self._root)

        self.joint_rotations = [None] * (self.joint_count)
        for i in range(self.joint_count):
            self.joint_rotations[i] = Klfa.JointRotation(self._io, self, self._root)


    class JointTranslation(KaitaiStruct):
        def __init__(self, _io, _parent=None, _root=None):
            self._io = _io
            self._parent = _parent
            self._root = _root if _root else self
            self._read()

        def _read(self):
            self.keyframe_count = self._io.read_u2le()
            self.keyframes = [None] * (self.keyframe_count)
            for i in range(self.keyframe_count):
                self.keyframes[i] = self._io.read_u2le()

            self.keyframe_total = self._io.read_u2le()
            self.coordinates = [None] * (self.keyframe_count)
            for i in range(self.keyframe_count):
                self.coordinates[i] = Klfa.Coordinate(self._io, self, self._root)



    class JointRotation(KaitaiStruct):
        def __init__(self, _io, _parent=None, _root=None):
            self._io = _io
            self._parent = _parent
            self._root = _root if _root else self
            self._read()

        def _read(self):
            self.keyframe_count = self._io.read_u2le()
            self.keyframes = [None] * (self.keyframe_count)
            for i in range(self.keyframe_count):
                self.keyframes[i] = self._io.read_u2le()

            self.keyframe_total = self._io.read_u2le()
            self.rotations = [None] * (self.keyframe_count)
            for i in range(self.keyframe_count):
                self.rotations[i] = Klfa.Rotation(self._io, self, self._root)



    class MorphKeyframeData(KaitaiStruct):
        """Morph weights are defined for each frame in the animation for
        animations that do have morph animations.
        
        morph0 and morph1 decides the index of the morph (klfz) to use and
        are essentially "inverses" of each other.
        If morph0 equals 0, morph1 equals 2, and weight equals 0x10,
        klfz #0 will have a weight of 0xEF and klfz #2 will have a weight
        of 0x10.
        
        Multiple morphs can be used per frame in case an animation needs
        to animate a character's face and hand morphs at the same time.
        """
        def __init__(self, _io, _parent=None, _root=None):
            self._io = _io
            self._parent = _parent
            self._root = _root if _root else self
            self._read()

        def _read(self):
            self.morph0 = self._io.read_u1()
            self.morph1 = self._io.read_u1()
            self.weight = self._io.read_u1()


    class Rotation(KaitaiStruct):
        """To get an euler angle, divide an axis by 0xFFFF (65535) and multiply it
        by 365.
        Y and Z are also inverted for this.
        """
        def __init__(self, _io, _parent=None, _root=None):
            self._io = _io
            self._parent = _parent
            self._root = _root if _root else self
            self._read()

        def _read(self):
            self.x = self._io.read_u2le()
            self.y = self._io.read_u2le()
            self.z = self._io.read_u2le()


    class FloatCoordinate(KaitaiStruct):
        def __init__(self, _io, _parent=None, _root=None):
            self._io = _io
            self._parent = _parent
            self._root = _root if _root else self
            self._read()

        def _read(self):
            self.x = self._io.read_f4le()
            self.y = self._io.read_f4le()
            self.z = self._io.read_f4le()


    class MorphKeyframe(KaitaiStruct):
        def __init__(self, _io, _parent=None, _root=None):
            self._io = _io
            self._parent = _parent
            self._root = _root if _root else self
            self._read()

        def _read(self):
            self.keyframe_data = [None] * (self._parent.morph_count)
            for i in range(self._parent.morph_count):
                self.keyframe_data[i] = Klfa.MorphKeyframeData(self._io, self, self._root)



    class Coordinate(KaitaiStruct):
        """Multiply these by the scale value in the header.
        The coordinates are Y-UP, but remember: Y and Z are inverted!
        """
        def __init__(self, _io, _parent=None, _root=None):
            self._io = _io
            self._parent = _parent
            self._root = _root if _root else self
            self._read()

        def _read(self):
            self.x = self._io.read_s2le()
            self.y = self._io.read_s2le()
            self.z = self._io.read_s2le()


    class MorphData(KaitaiStruct):
        def __init__(self, _io, _parent=None, _root=None):
            self._io = _io
            self._parent = _parent
            self._root = _root if _root else self
            self._read()

        def _read(self):
            self.morph_count = self._io.read_u1()
            self.morph_keyframes = [None] * (self._parent.keyframe_count)
            for i in range(self._parent.keyframe_count):
                self.morph_keyframes[i] = Klfa.MorphKeyframe(self._io, self, self._root)



    @property
    def morphs(self):
        if hasattr(self, '_m_morphs'):
            return self._m_morphs if hasattr(self, '_m_morphs') else None

        if self.morphs_offset != 0:
            _pos = self._io.pos()
            self._io.seek(self.morphs_offset)
            self._m_morphs = Klfa.MorphData(self._io, self, self._root)
            self._io.seek(_pos)

        return self._m_morphs if hasattr(self, '_m_morphs') else None


class CharacterIDs(Enum):
    CHR_AA = 0   # Lolo
    CHR_AD = 3   # Popka
    CHR_KL = 371 # Klonoa

def read_u16le(buf, offset) -> int:
    return struct.unpack("h", buf[offset:offset+2])[0]

def read_u32le(buf, offset) -> int:
    return struct.unpack("<I", buf[offset:offset+4])[0]

def read_offset_table(buf, offset, add_offset=False) -> List[int]:
    offsets = []
    offset_count = read_u32le(buf, offset)
    for i in range(offset_count):
        offsets.append((offset if add_offset else 0) + read_u32le(buf, offset + (i + 1) * 4))
    return offsets

def get_archive_size(offsets, index):
    return offsets[index + 1] - offsets[index]

def read_model_animations(buf, offset, char_id):
    lod_offsets = read_offset_table(buf, offset, True)
    data_offsets = read_offset_table(buf, lod_offsets[0], True)
    animation_offsets = read_offset_table(buf, data_offsets[3], True)[1:] # Skip first file in archive
    duplicate_offsets = []
    for animation in animation_offsets:
        if animation in duplicate_offsets:
            continue
        else:
            duplicate_offsets.append(animation)
        try:
            klfa = Klfa.from_bytes(buf[animation:]) # this is really inefficient
        except:
            continue
        if klfa.morphs == None:
            continue
        if klfa.name not in output[char_id]:
            morph_data = []
            for i in range(klfa.morphs.morph_count):
                keyframes = []
                for keyframe in klfa.morphs.morph_keyframes:
                    keyframes.append({"start": keyframe.keyframe_data[i].morph0, "end": keyframe.keyframe_data[i].morph1, "weight": keyframe.keyframe_data[i].weight / 0xFF})
                morph_data.append(keyframes)
            output[char_id][klfa.name] = morph_data

if __name__ == "__main__":
    headpack_buf = bytearray(open(headpack_bin_path, "rb").read())
    kldata_buf = bytearray(open(kldata_bin_path, "rb").read())
    headpack = Headpack.from_bytes(headpack_buf)
    output = {
        "CHR_AA": {},
        "CHR_AD": {},
        "CHR_KL": {},
    }

    for i, archive in enumerate(headpack.kldata.archives[:-2]):
        if i < 3 or i & 1 == 0:
            continue
        print(f"Processing archive {i}")
        archive_start = archive.offset
        archive_end = archive.offset + archive.size
        archive_bytes = kldata_buf[archive_start:archive_end]
        root_offsets = read_offset_table(archive_bytes, 0)
        if get_archive_size(root_offsets, 0) <= 16:
            continue
        assets_offsets = read_offset_table(archive_bytes, root_offsets[0], True)
        models_offsets = read_offset_table(archive_bytes, assets_offsets[1], True)
        if get_archive_size(models_offsets, CharacterIDs.CHR_AA.value) != 0:
            read_model_animations(archive_bytes, models_offsets[CharacterIDs.CHR_AA.value], CharacterIDs.CHR_AA.name)
        if get_archive_size(models_offsets, CharacterIDs.CHR_AD.value) != 0:
            read_model_animations(archive_bytes, models_offsets[CharacterIDs.CHR_AD.value], CharacterIDs.CHR_AD.name)
        if get_archive_size(models_offsets, CharacterIDs.CHR_KL.value) != 0:
            read_model_animations(archive_bytes, models_offsets[CharacterIDs.CHR_KL.value], CharacterIDs.CHR_KL.name)

    with open("animations.json", "w") as f:
        f.write(json.dumps(output, indent=4))
    