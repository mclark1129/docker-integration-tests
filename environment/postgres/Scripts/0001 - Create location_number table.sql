CREATE TABLE location_numbers (
	id SERIAL    
	, location_id UUID NOT NULL
	, number INTEGER
	, UNIQUE (location_id)
)	
