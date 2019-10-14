using System;

namespace WrestlingSimulator
{
	public struct NodeIndex
	{
		//sbyte is specifically used here as opposed to int simply because it takes up less memory.

		public sbyte x, y; //sbyte to allow us to use negative indexes to subtract from other indexes

		public NodeIndex (sbyte x, sbyte y) {
			this.x = x;
			this.y = y;
		}

		public static NodeIndex operator + (NodeIndex a) {
			return a;
		}

		public static NodeIndex operator - (NodeIndex a) {
			return new NodeIndex ((sbyte)-a.x, (sbyte)-a.y);
		}

		public static NodeIndex operator - (NodeIndex a, NodeIndex b) {
			return new NodeIndex ((sbyte)(a.x - b.x), (sbyte)(a.y - b.y));
		}

		public static NodeIndex operator + (NodeIndex a, NodeIndex b) {
			return new NodeIndex ((sbyte)(a.x + b.x), (sbyte)(a.y + b.y));
		}
	}
}

