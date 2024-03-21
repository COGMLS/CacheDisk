﻿namespace CacheDiskLib
{
	public enum CacheType
	{
		UNKNOWN,
		COPY,
		MOVE,
	}

	public enum CacheStatus
	{
		UNKNOWN,
		NOT_CACHED,
		CACHED,
		FAIL_TO_CACHE,
		FAIL_TO_RESTORE,
		FAIL_TO_REVERT
	}

	public enum FindCache
	{
		Path,
		Id,
		CachePath,
		BackupPath
	}

	public enum CacheDiskErrorCodes
	{
		SUCCESS,
		SOURCE_PATH_NOT_EXIST,
		CACHEDISK_PATH_NOT_EXIST,
		FAIL_TO_SET_BACKUP_PATH
	}

	public enum CacheIdErrorCodes
	{
		CREATED,
		ALREADY_EXIST,
		CACHE_ID_DOES_NOT_EXIST,
		FAIL_TO_GENERATE_CACHE_ID,
		FAIL_TO_CHECK_CACHE_ID,
		FAIL_ID_MISS_MATCH
	}

	public enum CacheDiskRegisterErrorCodes
	{
		PATH_NOT_FOUND,
		CACHE_DISK_PATH_NOT_FOUND,
		BACKUP_PATH_NOT_FOUND,
		CACHE_ID_NOT_FOUND,
		PATH_NOT_WRITE_CORRECT,
		CACHE_DISK_PATH_NOT_WRITE_CORRECT,
		BACKUP_PATH_NOT_WRITE_CORRECT,
		CACHE_ID_NOT_WRITE_CORRECT,
		CACHE_TYPE_NOT_WRITE_CORRECT,
		CACHE_TYPE_NOT_FOUND,
		CACHE_STATUS_NOT_WRITE_CORRECT,
		CACHE_STATUS_NOT_FOUND,
		REGISTER_FILE_MISSING,
		REGISTER_FILE_EMPTY,
		REGISTER_FILE_NOT_OPENED,
		REGISTER_FILE_FAIL_TO_CREATE,
		REGISTER_FILE_NOT_FOUND,
		REGISTER_FILE_CANT_BE_READ,
		REGISTER_FILE_CANT_BE_WRITE,
		REGISTER_FILE_CANT_CHECK_RECORD_DATA,
		REGISTER_FILE_CANT_BE_OPEN,
		REGISTER_FILE_CANT_BE_CLOSE,
		REGISTER_FILE_CANT_BE_DELETED
	}
}
