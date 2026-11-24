using Microsoft.EntityFrameworkCore; public static class UnsafeQuery { public static IQueryable<T> All<T>(IQueryable<T> query) where T : class => query.IgnoreQueryFilters(); }
