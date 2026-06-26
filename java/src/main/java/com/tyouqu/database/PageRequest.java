package com.tyouqu.database;

public record PageRequest(int pageIndex, int pageSize) {
    public PageRequest {
        if (pageIndex < 1) {
            throw new IllegalArgumentException("pageIndex must be greater than or equal to 1.");
        }
        if (pageSize < 1) {
            throw new IllegalArgumentException("pageSize must be greater than or equal to 1.");
        }
    }

    public int offset() {
        return (pageIndex - 1) * pageSize;
    }
}
