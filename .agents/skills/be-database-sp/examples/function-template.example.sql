-- =============================================================================
-- Función de Lectura de Alto Rendimiento (Read Model)
-- Patrón: LANGUAGE SQL STABLE SECURITY INVOKER
-- =============================================================================

CREATE OR REPLACE FUNCTION get_feature_summary_report(
    p_company_id UUID,
    p_start_date TIMESTAMPTZ,
    p_end_date TIMESTAMPTZ
)
RETURNS TABLE (
    item_id UUID,
    item_code VARCHAR,
    item_name VARCHAR,
    total_transactions BIGINT,
    total_amount NUMERIC
)
LANGUAGE SQL
STABLE
SECURITY INVOKER
AS $$
    SELECT 
        i.id AS item_id,
        i.code AS item_code,
        i.name AS item_name,
        COUNT(t.id) AS total_transactions,
        COALESCE(SUM(t.amount), 0) AS total_amount
    FROM items i
    LEFT JOIN transactions t 
        ON t.item_id = i.id 
        AND t.created_at >= p_start_date 
        AND t.created_at <= p_end_date
    WHERE i.company_id = p_company_id
      AND i.is_active = TRUE
    GROUP BY i.id, i.code, i.name
    ORDER BY total_amount DESC;
$$;
