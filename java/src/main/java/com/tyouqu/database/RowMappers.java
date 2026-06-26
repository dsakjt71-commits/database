package com.tyouqu.database;

import java.lang.reflect.Constructor;
import java.lang.reflect.Method;
import java.lang.reflect.Modifier;
import java.lang.reflect.RecordComponent;
import java.sql.ResultSet;
import java.sql.ResultSetMetaData;
import java.sql.SQLException;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Locale;
import java.util.Map;

final class RowMappers {
    private RowMappers() {
    }

    static <T> List<T> mapAll(ResultSet resultSet, Class<T> type) throws SQLException {
        List<T> rows = new ArrayList<>();
        while (resultSet.next()) {
            rows.add(mapOne(resultSet, type));
        }
        return rows;
    }

    @SuppressWarnings("unchecked")
    static <T> T mapOne(ResultSet resultSet, Class<T> type) throws SQLException {
        if (Map.class.isAssignableFrom(type)) {
            return (T) toMap(resultSet);
        }
        if (isScalar(type)) {
            return convert(resultSet.getObject(1), type);
        }
        Map<String, Object> values = toMap(resultSet);
        if (type.isRecord()) {
            return mapRecord(values, type);
        }
        return mapBean(values, type);
    }

    private static Map<String, Object> toMap(ResultSet resultSet) throws SQLException {
        ResultSetMetaData metaData = resultSet.getMetaData();
        Map<String, Object> values = new HashMap<>();
        for (int i = 1; i <= metaData.getColumnCount(); i++) {
            String label = metaData.getColumnLabel(i);
            values.put(normalize(label), resultSet.getObject(i));
        }
        return values;
    }

    private static <T> T mapRecord(Map<String, Object> values, Class<T> type) {
        try {
            RecordComponent[] components = type.getRecordComponents();
            Class<?>[] parameterTypes = new Class<?>[components.length];
            for (int i = 0; i < components.length; i++) {
                parameterTypes[i] = components[i].getType();
            }
            Constructor<T> constructor = type.getDeclaredConstructor(parameterTypes);
            constructor.setAccessible(true);
            Object[] args = new Object[components.length];
            for (int i = 0; i < components.length; i++) {
                args[i] = convert(values.get(normalize(components[i].getName())), components[i].getType());
            }
            return constructor.newInstance(args);
        } catch (ReflectiveOperationException ex) {
            throw new TyouquDatabaseException("Failed to map row to record. Type=" + type.getName(), ex);
        }
    }

    private static <T> T mapBean(Map<String, Object> values, Class<T> type) {
        try {
            Constructor<T> constructor = type.getDeclaredConstructor();
            constructor.setAccessible(true);
            T instance = constructor.newInstance();
            for (Method method : type.getMethods()) {
                if (!isSetter(method)) {
                    continue;
                }
                String property = normalize(method.getName().substring(3));
                if (values.containsKey(property)) {
                    method.invoke(instance, convert(values.get(property), method.getParameterTypes()[0]));
                }
            }
            return instance;
        } catch (ReflectiveOperationException ex) {
            throw new TyouquDatabaseException("Failed to map row to bean. Type=" + type.getName(), ex);
        }
    }

    private static boolean isSetter(Method method) {
        return Modifier.isPublic(method.getModifiers())
            && method.getName().startsWith("set")
            && method.getName().length() > 3
            && method.getParameterCount() == 1;
    }

    private static boolean isScalar(Class<?> type) {
        return type == String.class
            || type == Integer.class || type == int.class
            || type == Long.class || type == long.class
            || type == Boolean.class || type == boolean.class
            || type == Double.class || type == double.class
            || type == Float.class || type == float.class
            || type == Short.class || type == short.class
            || type == Byte.class || type == byte.class;
    }

    @SuppressWarnings("unchecked")
    private static <T> T convert(Object value, Class<T> targetType) {
        if (value == null) {
            return targetType.isPrimitive() ? primitiveDefault(targetType) : null;
        }
        if (targetType.isInstance(value)) {
            return targetType.cast(value);
        }
        if (targetType == String.class) {
            return targetType.cast(String.valueOf(value));
        }
        if (value instanceof Number number) {
            if (targetType == Integer.class || targetType == int.class) {
                return (T) Integer.valueOf(number.intValue());
            }
            if (targetType == Long.class || targetType == long.class) {
                return (T) Long.valueOf(number.longValue());
            }
            if (targetType == Double.class || targetType == double.class) {
                return (T) Double.valueOf(number.doubleValue());
            }
            if (targetType == Float.class || targetType == float.class) {
                return (T) Float.valueOf(number.floatValue());
            }
            if (targetType == Short.class || targetType == short.class) {
                return (T) Short.valueOf(number.shortValue());
            }
            if (targetType == Byte.class || targetType == byte.class) {
                return (T) Byte.valueOf(number.byteValue());
            }
        }
        if (targetType == Boolean.class || targetType == boolean.class) {
            return (T) Boolean.valueOf(Boolean.parseBoolean(String.valueOf(value)));
        }
        return targetType.cast(value);
    }

    @SuppressWarnings("unchecked")
    private static <T> T primitiveDefault(Class<T> type) {
        if (type == boolean.class) {
            return (T) Boolean.FALSE;
        }
        if (type == char.class) {
            return (T) Character.valueOf('\0');
        }
        if (type == long.class) {
            return (T) Long.valueOf(0);
        }
        if (type == double.class) {
            return (T) Double.valueOf(0);
        }
        if (type == float.class) {
            return (T) Float.valueOf(0);
        }
        if (type == short.class) {
            return (T) Short.valueOf((short) 0);
        }
        if (type == byte.class) {
            return (T) Byte.valueOf((byte) 0);
        }
        return (T) Integer.valueOf(0);
    }

    private static String normalize(String value) {
        return value.replace("_", "").toLowerCase(Locale.ROOT);
    }
}
