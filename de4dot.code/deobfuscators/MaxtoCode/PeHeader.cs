/*
    Copyright (C) 2011-2015 de4dot@gmail.com

    This file is part of de4dot.

    de4dot is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    de4dot is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with de4dot.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Diagnostics;
using System.Text;
using dnlib.PE;

namespace de4dot.code.deobfuscators.MaxtoCode {
	enum EncryptionVersion {
		Unknown,
		V1,
		V2,
		V3,
		V4,
		V5,
		V6,
		V7,
		V8,
	}

	class PeHeader {
		EncryptionVersion version;
		byte[] headerData;
		uint xorKey;

		public EncryptionVersion EncryptionVersion => version;

		public PeHeader(MainType mainType, MyPEImage peImage) {
			version = GetHeaderOffsetAndVersion(peImage, out uint headerOffset);
			headerData = peImage.OffsetReadBytes(headerOffset, 0x1000);
			// MC uses 4-byte xorKey, 2 Hex for 1 Byte
			GuessXorKey(false, peImage, 4);

			switch (version) {
			case EncryptionVersion.V1:
			case EncryptionVersion.V2:
			case EncryptionVersion.V3:
			case EncryptionVersion.V4:
			case EncryptionVersion.V5:
			default:
				xorKey = 0x7ABF931;
				break;

			case EncryptionVersion.V6:
				xorKey = 0x7ABA931;
				break;

			case EncryptionVersion.V7:
				xorKey = 0x8ABA931;
				break;

			case EncryptionVersion.V8:
				if (CheckMcKeyRva(peImage, 0x99BA9A13))
					break;
				if (CheckMcKeyRva(peImage, 0x18ABA931))
					break;
				if (CheckMcKeyRva(peImage, 0x18ABA933))
					break;
				break;
			}
		}

		bool CheckMcKeyRva(MyPEImage peImage, uint newXorKey) {
			xorKey = newXorKey;
			uint rva = GetMcKeyRva();
			return (rva & 0xFFF) == 0 && peImage.FindSection((RVA)rva) != null;
		}

		public uint GetMcKeyRva() => GetRva(0x0FFC, xorKey);
		public uint GetRva(int offset, uint xorKey) => ReadUInt32(offset) ^ xorKey;
		public uint ReadUInt32(int offset) => BitConverter.ToUInt32(headerData, offset);

		/// <summary>Guess the xorKey by the Blue-force Attack in here
		/// <para>With optimization inherited by the de4dot's API.
		/// <para>Use Python script for general purpose XOR guessing.
		/// <para>Multiple parameters
		/// <param name="turnOn">Used to enable this method.</param>
		/// <param name="peImage">Used to pass the peImage param.</param>
		/// <param name="keyLength">Used to specify the keyLength of the possible xorKey value.(In bytes)</param>
		/// <para>(keyLength is not the length of the key)
		/// <para>Try these xorKey in PEHeader() <see cref="PeHeader(MainType, MyPEImage)"/>, if wrong, error message of "Invalid resource RVA and size found" will be given.
		/// <para>Author: Tianjiao(Wang Genghuang) at https://github.com/Tianjiao/de4dot
		/// </summary>
		private void GuessXorKey(bool turnOn, in MyPEImage peImage, ushort keyLength) {
			if (turnOn != true)
				return;

			string lowerBandHex = "", upperBandHex = "";
			var lowerBandHexSB = new StringBuilder("0x1", (keyLength + 3));
			var upperBandHexSB = new StringBuilder("0x", ((keyLength * 2) + 2));

			// Minimum Hex value for this keyLength
			if (lowerBandHexSB != null)
				lowerBandHex = lowerBandHexSB.Append('0', keyLength).ToString();

			// Maximum Hex value for this keyLength
			if (upperBandHexSB != null)
				upperBandHex = upperBandHexSB.Append('F', (keyLength * 2)).ToString();

			var stopWatch = new Stopwatch();
			stopWatch.Start();
			ExcludeXorKey(peImage, lowerBandHex, upperBandHex, false);
			stopWatch.Stop();
			// Get the elapsed time as a TimeSpan value.
			var ts = stopWatch.Elapsed;

			// Format and display the TimeSpan value.
			string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
			Logger.vv("Finish Guessing xorkey, RunTime " + elapsedTime);
		}

		/// <summary>Exclude the xorKey by CheckMcKeyRva() <see cref="CheckMcKeyRva(MyPEImage, uint)"/>
		/// <para>With optimization inherited by the de4dot's API.
		/// <para>Multiple parameters
		/// <param name="peImage">Used to pass the peImage param.</param>
		/// <param name="lowerBandHex">Used to specify the Minimum Hex value of the possible xorKey value.</param>
		/// <param name="upperBandHex">Used to specify the Maximum Hex value of the possible xorKey value.</param>
		/// <param name="isStringMatching">Used to specify whether enable String Matching.</param>
		/// <para>Author: Tianjiao(Wang Genghuang) at https://github.com/Tianjiao/de4dot
		/// </summary>
		private void ExcludeXorKey(in MyPEImage peImage, string lowerBandHex, string upperBandHex, bool isStringMatching) {
			uint lowerBand = 0, upperBand = 0;
			if (lowerBandHex != null)
				lowerBand = Convert.ToUInt32(lowerBandHex, 16);
			if (upperBandHex != null)
				upperBand = Convert.ToUInt32(upperBandHex, 16);

			for (uint triedXorKey = lowerBand; triedXorKey < upperBand; triedXorKey++) {
				if (CheckMcKeyRva(peImage, triedXorKey)) {
					if (isStringMatching) {
						StringMatching(peImage, triedXorKey);
						break;
					}
					Logger.vv("Guessed possible xorkey found at");
					Logger.Instance.Indent();
					Logger.vv("0x" + triedXorKey.ToString("X"));
					Logger.Instance.DeIndent();
					Logger.vv("_________________________________");
				}

				if (triedXorKey >= upperBand)
					break;
			}
		}

		/// <summary>Guess the xorKey by the existing StringMatching in here
		/// <para>Alpha, on EncryptionVersion.V8 above
		/// <para>Multiple parameters
		/// <param name="peImage">Used to pass the peImage param.</param>
		/// <param name="firstFoundXorKey">Used to specify the firstFoundXorKey string for known pattern.</param>
		/// <para>Author: Tianjiao(Wang Genghuang) at https://github.com/Tianjiao/de4dot
		/// </summary>
		private void StringMatching(in MyPEImage peImage, uint firstFoundXorKey) {

			string firstXorKeyHex = "0x" + firstFoundXorKey.ToString("X");
			int indexOf = 0;
			if (firstXorKeyHex != null)
				indexOf = firstXorKeyHex.Length / 2;

			var hex = new char[16] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
			var sb = new StringBuilder(firstXorKeyHex);
			uint patternKeyValue = 0;

			for (int i = 0; i < hex.Length; i++) {
				for (int k = 0; k < hex.Length; k++) {
					if ((sb != null) && (indexOf >= 0) && (indexOf + 1 < sb.Length)) {
						sb[indexOf] = hex[i];
						sb[indexOf + 1] = hex[k];
					}
					string patternKey = sb.ToString();
					if (patternKey != null)
						patternKeyValue = Convert.ToUInt32(patternKey, 16);

					if (CheckMcKeyRva(peImage, patternKeyValue)) {
						Logger.vv("Guessed possible xorkey by PatternMatching found at");
						Logger.Instance.Indent();
						Logger.vv("0x" + patternKeyValue.ToString("X"));
						Logger.Instance.DeIndent();
						Logger.vv("_________________________________");
					}
					sb = new StringBuilder(firstXorKeyHex);
				}
			}
		}

		static EncryptionVersion GetHeaderOffsetAndVersion(MyPEImage peImage, out uint headerOffset) {
			headerOffset = 0;

			var version = GetVersion(peImage, headerOffset);
			if (version != EncryptionVersion.Unknown)
				return version;

			var section = peImage.FindSection(".rsrc");
			if (section != null) {
				version = GetHeaderOffsetAndVersion(section, peImage, out headerOffset);
				if (version != EncryptionVersion.Unknown)
					return version;
			}

			foreach (var section2 in peImage.Sections) {
				version = GetHeaderOffsetAndVersion(section2, peImage, out headerOffset);
				if (version != EncryptionVersion.Unknown)
					return version;
			}

			return EncryptionVersion.Unknown;
		}

		static EncryptionVersion GetHeaderOffsetAndVersion(ImageSectionHeader section, MyPEImage peImage, out uint headerOffset) {
			headerOffset = section.PointerToRawData;
			uint end = section.PointerToRawData + section.SizeOfRawData - 0x1000 + 1;
			while (headerOffset < end) {
				var version = GetVersion(peImage, headerOffset);
				if (version != EncryptionVersion.Unknown)
					return version;
				headerOffset++;
			}

			return EncryptionVersion.Unknown;
		}

		static EncryptionVersion GetVersion(MyPEImage peImage, uint headerOffset) {
			uint m1lo = peImage.OffsetReadUInt32(headerOffset + 0x900);
			uint m1hi = peImage.OffsetReadUInt32(headerOffset + 0x904);

			// Key reader of Rva900h keys, just the possible combinations.
			// Two methods: Call hundreds of staffs to build and run it respectively
			// Or use Automated scripts to run msbuild then run all instances of de4dot

			/*
			if (m1lo > 0 && m1hi > 0) {
			// Print Possible MagicLo from Rva900h
				Logger.vv("The MagicLo from Rva900h could be");
				Logger.Instance.Indent();
				Logger.vv("MagicLo = 0x" + m1lo.ToString("X"));
				Logger.Instance.DeIndent();

			// Print Possible MagicHi from Rva900h			
				Logger.vv("The MagicHi from Rva900h could be");
				Logger.Instance.Indent();
				Logger.vv("MagicHi = 0x" + m1hi.ToString("X"));
				Logger.Instance.DeIndent();
				Logger.vv("_________________________________");
			}
			*/

			foreach (var info in EncryptionInfos.Rva900h) {
				if (info.MagicLo == m1lo && info.MagicHi == m1hi) {
					// Print Successful MagicLo from Rva900h
					Logger.vv("The used MagicLo from Rva900h is");
					Logger.Instance.Indent();
					Logger.vv("MagicLo = 0x" + m1lo.ToString("X"));
					Logger.Instance.DeIndent();

					// Print Successful MagicHi from Rva900h			
					Logger.vv("The used MagicHi from Rva900h is");
					Logger.Instance.Indent();
					Logger.vv("MagicHi = 0x" + m1hi.ToString("X"));
					Logger.Instance.DeIndent();
					Logger.vv("_________________________________");

					Logger.vv("Check these keys in EncryptionInfo[] Rva900h in de4dot.code\\deobfuscators\\MaxtoCode\\EncryptionInfos.cs");

					return info.Version;
				}
			}

			return EncryptionVersion.Unknown;
		}
	}
}
