﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace FreeSql.Internal.Model {
	class TableInfo {
		public Type Type { get; set; }
		public Type TypeLazy { get; set; }
		public MethodInfo TypeLazySetOrm { get; set; }
		public Dictionary<string, PropertyInfo> Properties { get; set; } = new Dictionary<string, PropertyInfo>(StringComparer.CurrentCultureIgnoreCase);
		public Dictionary<string, ColumnInfo> Columns { get; set; } = new Dictionary<string, ColumnInfo>(StringComparer.CurrentCultureIgnoreCase);
		public Dictionary<string, ColumnInfo> ColumnsByCs { get; set; } = new Dictionary<string, ColumnInfo>(StringComparer.CurrentCultureIgnoreCase);
		public ColumnInfo[] Primarys { get; set; }
		public string CsName { get; set; }
		public string DbName { get; set; }
		public string DbOldName { get; set; }
		public string SelectFilter { get; set; }


		ConcurrentDictionary<string, TableRef> _refs { get; } = new ConcurrentDictionary<string, TableRef>(StringComparer.CurrentCultureIgnoreCase);

		internal void AddOrUpdateTableRef(string propertyName, TableRef tbref) {
			_refs.AddOrUpdate(propertyName, tbref, (ok, ov) => tbref);
		}
		internal TableRef GetTableRef(string propertyName) {
			if (_refs.TryGetValue(propertyName, out var tryref) == false) return null;
			if (tryref.Exception != null) throw tryref.Exception;
			return tryref;
		}
	}

	internal class TableRef {
		public PropertyInfo Property { get; set; }

		public TableRefType RefType { get; set; }

		public Type RefEntityType { get; set; }
		/// <summary>
		/// 中间表，多对多
		/// </summary>
		public Type RefMiddleEntityType { get; set; }

		public List<ColumnInfo> Columns { get; set; } = new List<ColumnInfo>();
		public List<ColumnInfo> MiddleColumns { get; set; } = new List<ColumnInfo>();
		public List<ColumnInfo> RefColumns { get; set; } = new List<ColumnInfo>();

		public Exception Exception { get; set; }
	}
	internal enum TableRefType {
		OneToOne, ManyToOne, OneToMany, ManyToMany
	}
}