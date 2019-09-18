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

namespace de4dot.blocks {
	[Serializable]
	public class DumpedMethod {
		[NonSerialized]
		public ushort mhFlags;          // method header Flags
		[NonSerialized]
		public ushort mhMaxStack;       // method header MaxStack
		[NonSerialized]
		public uint mhCodeSize;         // method header CodeSize
		[NonSerialized]
		public uint mhLocalVarSigTok;   // method header LocalVarSigTok

		[NonSerialized]
		public uint mdRVA;              // methodDef RVA
		[NonSerialized]
		public ushort mdImplFlags;      // methodDef ImplFlags
		[NonSerialized]
		public ushort mdFlags;          // methodDef Flags
		[NonSerialized]
		public uint mdName;             // methodDef Name (index into #String)
		[NonSerialized]
		public uint mdSignature;        // methodDef Signature (index into #Blob)
		[NonSerialized]
		public uint mdParamList;        // methodDef ParamList (index into Param table)

		[NonSerialized]
		public uint token;              // metadata token

		[NonSerialized]
		public byte[] code;
		[NonSerialized]
		public byte[] extraSections;
	}
}
