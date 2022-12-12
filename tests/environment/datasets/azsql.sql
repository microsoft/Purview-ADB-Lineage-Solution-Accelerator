CREATE SCHEMA nondbo

CREATE TABLE nondbo.exampleInputC (
id int
,cityPopulation int
)

CREATE TABLE dbo.exampleInputB (
id int
,city varchar(30)
,stateAbbreviation varchar(2)
)

CREATE TABLE dbo.exampleInputA (
id int
,postalcode varchar(5)
,street varchar(50)
)

INSERT INTO nondbo.exampleInputC(id, cityPopulation)
VALUES(1, 1000)

INSERT INTO dbo.exampleInputB(id, city, stateAbbreviation)
VALUES(1, 'Springfield', '??')

INSERT INTO dbo.exampleInputA(id, postalcode, street)
VALUES(1, '55555', '742 Evergreen Terrace')
