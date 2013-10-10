' This code has been provided by Andrew Conrad from Microsoft
' See http://blogs.msdn.com/aconrad/archive/2007/09/07/science-project.aspx

Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Reflection
Imports System.ComponentModel
Imports System.ComponentModel.DataAnnotations

Public Class ObjectShredder(Of T)
    Private _fi As FieldInfo()
    Private _pi As PropertyInfo()
    Private _ordinalMap As Dictionary(Of String, Integer)
    Private _ordinalNameMap As Dictionary(Of String, String)
    Private _type As Type

    Public Sub New()
        _type = GetType(T)
        _fi = _type.GetFields()
        _pi = _type.GetProperties()
        _ordinalMap = New Dictionary(Of String, Integer)()
        _ordinalNameMap = New Dictionary(Of String, String)()
    End Sub

    Public Function Shred(ByVal source As IEnumerable(Of T), ByVal table As DataTable, _
      ByVal options As LoadOption?, Optional useDisplayNames As Boolean = False) As DataTable

        If GetType(T).IsPrimitive Then
            table = ShredPrimitive(source, table, options)
        End If

        If table Is Nothing Then
            table = New DataTable(GetType(T).Name)
        End If

        ' now see if need to extend datatable base on the type T + build ordinal map
        table = ExtendTableBaseClassFirst(table, GetType(T))

        table.BeginLoadData()
        Using e As IEnumerator(Of T) = source.GetEnumerator()
            While e.MoveNext()
                If options IsNot Nothing Then
                    table.LoadDataRow(ShredObject(table, e.Current), options.Value)
                Else
                    table.LoadDataRow(ShredObject(table, e.Current), True)
                End If
            End While
        End Using
        table.EndLoadData()
        If (useDisplayNames) Then
            RenameColumnsToDisplayName(table)
        End If
        Return table
    End Function


    Public Function ShredPrimitive(ByVal source As IEnumerable(Of T), _
                                   ByVal table As DataTable, ByVal options As LoadOption?) As DataTable
        If table Is Nothing Then
            table = New DataTable(GetType(T).Name)
        End If

        If Not table.Columns.Contains("Value") Then
            table.Columns.Add("Value", GetType(T))
        End If

        table.BeginLoadData()
        Using e As IEnumerator(Of T) = source.GetEnumerator()
            Dim values = New Object(table.Columns.Count - 1) {}
            While e.MoveNext()
                values(table.Columns("Value").Ordinal) = e.Current
                If Not options Is Nothing Then
                    table.LoadDataRow(values, options.Value)
                Else
                    table.LoadDataRow(values, True)
                End If
            End While
        End Using
        table.EndLoadData()
        Return table
    End Function

    Public Function RenameColumnsToDisplayName(ByVal table As DataTable) As DataTable

        For Each col In table.Columns
            If _ordinalNameMap.containskey(col.ColumnName) Then
                col.ColumnName = _ordinalNameMap(col.ColumnName)
            End If
        Next

        Return table
    End Function
    Public Function ExtendTableBaseClassFirst(ByVal table As DataTable, ByVal type As Type) As DataTable
        If (type.BaseType IsNot Nothing) Then
            table = ExtendTableBaseClassFirst(table, type.BaseType)
        End If

        For Each f As FieldInfo In type.GetFields()
            If (Not _ordinalMap.ContainsKey(f.Name)) Then
                Dim dc As DataColumn
                dc = If(table.Columns.Contains(f.Name), table.Columns(f.Name), table.Columns.Add(f.Name, f.FieldType))
                _ordinalMap.Add(f.Name, dc.Ordinal)
                _ordinalNameMap.Add(f.Name, f.Name)
            End If
        Next f

        For Each p As PropertyInfo In type.GetProperties()
            If Not _ordinalMap.ContainsKey(p.Name) Then
                Dim colType As Type = p.PropertyType
                If (colType.IsGenericType) AndAlso (colType.GetGenericTypeDefinition() Is GetType(Nullable(Of ))) Then
                    colType = colType.GetGenericArguments()(0)
                End If
                Dim dc As DataColumn = IIf(table.Columns.Contains(p.Name), table.Columns(p.Name), table.Columns.Add(p.Name, colType))
                _ordinalMap.Add(p.Name, dc.Ordinal)
                Dim name = p.Name
                Dim attr As DisplayNameAttribute = p.GetCustomAttributes(GetType(DisplayNameAttribute), True).SingleOrDefault()
                If (attr IsNot Nothing) Then
                    name = attr.DisplayName
                End If
                Dim attr2 As DisplayAttribute = p.GetCustomAttributes(GetType(DisplayAttribute), True).SingleOrDefault()
                If (attr2 IsNot Nothing) Then
                    name = attr2.Name
                End If
                _ordinalNameMap.Add(p.Name, name)
            End If
        Next


        Return table
    End Function

    'Public Function ExtendTable(ByVal table As DataTable, ByVal type As Type) As DataTable
    '    For Each f As FieldInfo In type.GetFields()
    '        If (Not _ordinalMap.ContainsKey(f.Name)) Then
    '            Dim dc As DataColumn
    '            dc = If(table.Columns.Contains(f.Name), table.Columns(f.Name), table.Columns.Add(f.Name, f.FieldType))
    '            _ordinalMap.Add(f.Name, dc.Ordinal)
    '        End If
    '    Next f

    '    For Each p As PropertyInfo In type.GetProperties()
    '        If Not _ordinalMap.ContainsKey(p.Name) Then
    '            Dim colType As Type = p.PropertyType
    '            If (colType.IsGenericType) AndAlso (colType.GetGenericTypeDefinition() Is GetType(Nullable(Of ))) Then
    '                colType = colType.GetGenericArguments()(0)
    '            End If
    '            Dim dc As DataColumn = IIf(table.Columns.Contains(p.Name), table.Columns(p.Name), table.Columns.Add(p.Name, colType))
    '            _ordinalMap.Add(p.Name, dc.Ordinal)
    '        End If
    '    Next

    '    Return table
    'End Function

    Public Function ShredObject(ByVal table As DataTable, ByVal instance As T) As Object()
        Dim fi As FieldInfo() = _fi
        Dim pi As PropertyInfo() = _pi

        If instance.GetType() IsNot GetType(T) Then
            ExtendTableBaseClassFirst(table, instance.GetType())
            fi = instance.GetType().GetFields()
            pi = instance.GetType().GetProperties()
        End If

        Dim values As Object() = New Object(table.Columns.Count - 1) {}
        For Each f As FieldInfo In fi
            values(_ordinalMap(f.Name)) = f.GetValue(instance)
        Next

        For Each p As PropertyInfo In pi
            values(_ordinalMap(p.Name)) = p.GetValue(instance, Nothing)
        Next
        Return values
    End Function
End Class