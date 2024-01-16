namespace CacheDiskLib
{
	enum CacheType
	{
		UNKNOWN,
		COPY,
		MOVE
	}

	enum FindCache
	{
		Path,
		Id,
		CachePath,
		BackupPath
	}

	enum CacheDiskErrorCodes
	{
		SUCCESS,
		SOURCE_PATH_NOT_EXIST,
		CACHEDISK_PATH_NOT_EXIST,
		FAIL_TO_SET_BACKUP_PATH
	}
}
