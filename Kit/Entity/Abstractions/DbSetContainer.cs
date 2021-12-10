using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Kit.Entity.Interfaces;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Linq.Expressions;
using System.Collections;
using System.Threading;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Kit.Entity.Abstractions
{

    public abstract class DbSetContainer
    {
        public readonly Type ContainedType;
        public readonly string TableName;
        public dynamic DbSet;
        protected DbSetContainer(Type containedType,dynamic dbSet)
        {
            this.ContainedType = containedType;
            this.TableName = DatabaseContext.GetTableName(ContainedType);
            this.DbSet=dbSet;
        }

        public static DbSetContainer Contain<T>(Microsoft.EntityFrameworkCore.DbSet<T> dbSet) where T : class
        {
            return new DbSetContainer<T>(typeof(T), dbSet);
        }
    }

    public class DbSetContainer<TEntity> : DbSetContainer where TEntity : class
    {
        public new readonly Microsoft.EntityFrameworkCore.DbSet<TEntity> DbSet;
        public DbSetContainer(Type containedType, Microsoft.EntityFrameworkCore.DbSet<TEntity> dbSet) : base(containedType,dbSet)
        {
            this.DbSet= dbSet;
        }

    }
}
