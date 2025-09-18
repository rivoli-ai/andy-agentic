-- Enable pgvector extension
CREATE EXTENSION IF NOT EXISTS vector;

-- Create a function to check if pgvector is available
CREATE OR REPLACE FUNCTION check_pgvector_availability()
RETURNS boolean AS $$
BEGIN
    -- Try to create a test vector column
    BEGIN
        CREATE TEMP TABLE test_vector_table (id serial, embedding vector(3));
        DROP TABLE test_vector_table;
        RETURN true;
    EXCEPTION
        WHEN OTHERS THEN
            RETURN false;
    END;
END;
$$ LANGUAGE plpgsql;

-- Log pgvector availability
DO $$
BEGIN
    IF check_pgvector_availability() THEN
        RAISE NOTICE 'pgvector extension is available and working correctly';
    ELSE
        RAISE WARNING 'pgvector extension is not available or not working correctly';
    END IF;
END $$;

-- Grant necessary permissions
GRANT ALL PRIVILEGES ON DATABASE agentic_db TO agentic_user;
GRANT ALL PRIVILEGES ON SCHEMA public TO agentic_user;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO agentic_user;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO agentic_user;
