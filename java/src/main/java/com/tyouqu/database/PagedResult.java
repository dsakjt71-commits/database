package com.tyouqu.database;

import java.util.List;

public record PagedResult<T>(
    List<T> items,
    long totalCount,
    int pageIndex,
    int pageSize
) {
}
