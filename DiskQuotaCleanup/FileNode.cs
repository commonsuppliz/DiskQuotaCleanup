using System;
using System.Collections.Generic;
using System.Text;
using Eto.Forms;

namespace DiskQuotaCleanup
{
	public class MySortBySizeSorter : System.Collections.Generic.IComparer<FolderNode>
	{
		private bool _reverse = false;
		public MySortBySizeSorter()
		{

		}
		public MySortBySizeSorter(bool b)
		{
			_reverse = b;
		}
		public int Compare(FolderNode f1, FolderNode f2)
		{
			if (f1 == null || f2 == null)
			{
				return -1;
			}
			if (_reverse == true)
			{
				return f1.Size.CompareTo(f2.Size);
			}
			else
			{
				return f2.Size.CompareTo(f1.Size);
			}



		}
	}
	public class FolderNode : TreeGridItem
	{
		public string FullPath = null;
		public long Size = 0;
		public DateTime LastModified;
		public int Depth = 0;
		//public object[] Fields = null;
		public DateTime CreatedTime;
		public DateTime LastAccessed;

		public FolderNode(string _dirPath, long _dirSize, DateTime _lastModified, DateTime _creationTime, DateTime _lastAccessed, int dirDepth)
		{
			this.FullPath = _dirPath;
			this.Size = _dirSize;
			this.LastModified = _lastModified;
			this.Depth = dirDepth;
			this.LastAccessed = _lastAccessed;
			this.CreatedTime = _creationTime;


		}
		public override string ToString()
		{
			return string.Format("{0} {1} {2}", FullPath, Size, LastModified.ToShortDateString());

		}


		public new object[] Values
		{
			get { return new object[] { this.FullPath, this.Size, this.LastModified, this.Depth }; }
		}
		private const double MegaBytes = 1024 * 1024;
		private const double GigaBytes = 1024 * 1024 * 1024;
		public override object GetValue(int column)
		{
			switch (column)
			{
				case 0:
					return this.FullPath;
				case 1:
					//return this.Size.ToString();
					if (this.Size > GigaBytes)
					{
						double _r = ((double)this.Size / GigaBytes);
						return _r.ToString("##.##") + "GB";
					}
					else if (this.Size > MegaBytes)
					{
						double _r = ((double)this.Size / MegaBytes);
						return _r.ToString("##.##") + "MB";
					}
					else if (this.Size > 1024)
					{
						double _r = ((double)this.Size / 1024);
						return _r.ToString("##.##") + "KB";
					}
					else
					{
						return this.Size.ToString();
					}
				case 2:
					return this.LastModified.ToShortDateString();
				case 3:
					return this.CreatedTime.ToShortDateString();
				case 4:
					return this.LastAccessed.ToShortDateString();
				case 5:
					return this.Depth;
				default:
					return null;
			}
			//return null;
		}
		public override void SetValue(int column, object value)
		{
			base.SetValue(column, value);
		}

	}
	public class MySortByNameSorter : System.Collections.Generic.IComparer<FolderNode>
	{
		private bool _reverse = false;
		public MySortByNameSorter()
		{


		}
		public MySortByNameSorter(bool b)
		{
			_reverse = b;
		}
		public int Compare(FolderNode f1, FolderNode f2)
		{
			if (f1 == null || f2 == null)
			{
				return -1;
			}
			if (!_reverse)
			{
				return string.Compare(f1.FullPath, f2.FullPath, StringComparison.Ordinal);
			}
			else
			{
				return string.Compare(f2.FullPath, f1.FullPath, StringComparison.Ordinal);
			}



		}
	}
}
