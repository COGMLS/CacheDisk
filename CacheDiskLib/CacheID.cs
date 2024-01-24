namespace CacheDiskLib
{
	public class CacheID
	{
		public readonly string ID;
		public readonly CacheIdErrorCodes status;

		public CacheID()
		{
			this.ID = CacheIdTools.GenerateUniqueCacheId(ref status);
		}

		public CacheID (string ID)
		{
			CacheIdErrorCodes status = CacheIdTools.GetIdStatus(ID);

			if (status == CacheIdErrorCodes.ALREADY_EXIST)
			{
				this.ID = ID;
				this.status = status;
			}
			else
			{
				this.ID = "";
				this.status = status;
			}
		}
	}
}
