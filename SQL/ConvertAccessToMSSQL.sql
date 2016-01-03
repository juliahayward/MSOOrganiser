-------------------------------------------------
-- Various tables need an ID
-------------------------------------------------

alter table [mso].[dbo].[Olympiad Info] add Id INT IDENTITY(1,1)

alter table mso.dbo.Fees add Id INT IDENTITY(1,1)

ALTER TABLE mso.dbo.Fees ADD PRIMARY KEY (Id)

alter table mso.dbo.Games add Id INT IDENTITY(1,1)


alter table [mso].[dbo].[Entrants] add OlympiadId INT 

update e set OlympiadId = o.Id 
FROM mso.dbo.entrants e
join mso.dbo.[Olympiad Info] o on e.year = o.yearof

alter table [mso].[dbo].Events add OlympiadId INT 

update e set OlympiadId = o.Id 
FROM mso.dbo.events e
join mso.dbo.[Olympiad Info] o on e.year = o.yearof

ALTER TABLE mso.dbo.Events ADD FOREIGN KEY (OlympiadId) REFERENCES [Olympiad Info] (Id)

alter table [mso].[dbo].Location add OlympiadId INT 
alter table mso.dbo.Location add Id INT IDENTITY(1,1)
ALTER TABLE mso.dbo.Location ADD PRIMARY KEY (Id)

update e set OlympiadId = o.Id 
FROM mso.dbo.locaTION e
join mso.dbo.[Olympiad Info] o on e.year = o.yearof

ALTER TABLE mso.dbo.Location ADD FOREIGN KEY (OlympiadId) REFERENCES [Olympiad Info] (Id)



create table [mso].[dbo].UserLogins
(
	Id INT IDENTITY(1,1) NOT NULL,
	UserId INT NOT NULL,
	LogInDate DATETIME NOT NULL,
	LogOutDate DATETIME NULL,
	CONSTRAINT [UserLogins$PrimaryKey] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) 
ON [PRIMARY]

ALTER TABLE mso.dbo.Arbiters ADD Id INT IDENTITY(1,1)

ALTER TABLE mso.dbo.Arbiters ADD PRIMARY KEY (Id)

---------------------------------------------------------
--- Don't store any credit card data, even if out of date
---------------------------------------------------------

ALTER TABLE mso.dbo.Payments
DROP CONSTRAINT [SSMA_CC$Payments$Card Address 1$disallow_zero_length],
[SSMA_CC$Payments$Card Address 2$disallow_zero_length],
[SSMA_CC$Payments$Card City$disallow_zero_length],
[SSMA_CC$Payments$Card Country$disallow_zero_length],
[SSMA_CC$Payments$Card County$disallow_zero_length],
[SSMA_CC$Payments$Card name$disallow_zero_length],
[SSMA_CC$Payments$Card number$disallow_zero_length],
[SSMA_CC$Payments$Card Post-code$disallow_zero_length],
[SSMA_CC$Payments$Expiry date$disallow_zero_length],
[SSMA_CC$Payments$Issue$disallow_zero_length]

DROP INDEX [Payments$Card Post-code] ON mso.dbo.Payments

ALTER TABLE mso.dbo.Payments
DROP COLUMN [Card Name], [Expiry Date], [Issue], [Card number], [Card Address 1], [Card Address 2], [Card City], [Card County], [Card Post-code], [Card Country]


---------------------------------------------------------
--- Payments - turn Year into Olympiad Id
---------------------------------------------------------

alter table [mso].[dbo].[Payments] add OlympiadId INT

update p set OlympiadId = o.Id 
FROM mso.dbo.payments p
join mso.dbo.[Olympiad Info] o on p.year = o.yearof

---------------------------------------------------------
--- Unify manually entered medals which are inconsistent
---------------------------------------------------------

UPDATE [mso].[dbo].[Entrants] SET Medal = 'Gold JNR' WHERE Medal = 'Jnr Gold'

UPDATE [mso].[dbo].[Entrants] SET Medal = 'Silver JNR' WHERE Medal = 'Jnr Silver'

UPDATE [mso].[dbo].[Entrants] SET Medal = 'Bronze JNR' WHERE Medal = 'Jnr Bronze'

---------------------------------------------------------
--- Arbiters - remove redundant Events.Arbiter column 
--- 1: remove duplicate information
--- 2: copy across all existing (except for ?? and None)
--- 3: remove redundant column
---------------------------------------------------------

update mso.dbo.events set arbiter = null where ein in (select ein from mso.dbo.arbiters)

INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 4984, EIN FROM mso.dbo.Events where Arbiter like '%Alan Farrell%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 4519, EIN FROM mso.dbo.Events where Arbiter like '%Andrew Havery%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 10049, EIN FROM mso.dbo.Events where Arbiter like '%Ann Chilcot%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 3470, EIN FROM mso.dbo.Events where Arbiter like '%Arnold Lutton%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 610, EIN FROM mso.dbo.Events where Arbiter like '%Aubrey de Grey%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 4289, EIN FROM mso.dbo.Events where Arbiter like '%Barbara Carey%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 1282, EIN FROM mso.dbo.Events where Arbiter like '%Barbara Corfe%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 9553, EIN FROM mso.dbo.Events where Arbiter like '%Bernard Morgan%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 868, EIN FROM mso.dbo.Events where Arbiter like '%Bill Hartston%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 8423, EIN FROM mso.dbo.Events where Arbiter like '%Bob Wade%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 8421, EIN FROM mso.dbo.Events where Arbiter like '%Bobby Raikhy%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 4377, EIN FROM mso.dbo.Events where Arbiter like '%Chris Keeley%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 264, EIN FROM mso.dbo.Events where Arbiter like '%CK Lai%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 8424, EIN FROM mso.dbo.Events where Arbiter like '%Dan Glimne%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 663, EIN FROM mso.dbo.Events where Arbiter like '%David Kotin%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 8415, EIN FROM mso.dbo.Events where Arbiter like '%David Levy%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 8422, EIN FROM mso.dbo.Events where Arbiter like '%David Lunn%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 10048, EIN FROM mso.dbo.Events where Arbiter like '%David Norman%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 8401, EIN FROM mso.dbo.Events where Arbiter like '%David Parlett%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 638, EIN FROM mso.dbo.Events where Arbiter like '%David Sedgwick%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 8402, EIN FROM mso.dbo.Events where Arbiter like '%Eric Johnson%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 5254, EIN FROM mso.dbo.Events where Arbiter like '%Gerald Jacobs%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 183, EIN FROM mso.dbo.Events where Arbiter like '%Marc Shaw%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 741, EIN FROM mso.dbo.Events where Arbiter like '%Neville Belinfante%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 4254, EIN FROM mso.dbo.Events where Arbiter like '%Glenda Trew%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 4251, EIN FROM mso.dbo.Events where Arbiter like '%Go Arbiter%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 10050, EIN FROM mso.dbo.Events where Arbiter like '%Ian Cawes%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 8373, EIN FROM mso.dbo.Events where Arbiter like '%Jack Berkovi%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 8425, EIN FROM mso.dbo.Events where Arbiter like '%Jim McCarthy%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 449, EIN FROM mso.dbo.Events where Arbiter like '%Josef Kollar%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 10051, EIN FROM mso.dbo.Events where Arbiter like '%Janet Green%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 8426, EIN FROM mso.dbo.Events where Arbiter like '%Kris Burm%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 8414, EIN FROM mso.dbo.Events where Arbiter like '%Lady Mary Tovey%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 4560, EIN FROM mso.dbo.Events where Arbiter like '%Lester Luer%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 8413, EIN FROM mso.dbo.Events where Arbiter like '%Malcolm%Carey%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 3252, EIN FROM mso.dbo.Events where Arbiter like '%Mark Stretch%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 9125, EIN FROM mso.dbo.Events where Arbiter like '%Martin Burroughs%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 8287, EIN FROM mso.dbo.Events where Arbiter like '%Michael Crane%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 10089, EIN FROM mso.dbo.Events where Arbiter like '%Michael Main%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 9493, EIN FROM mso.dbo.Events where Arbiter like '%Mike Wellman%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 264, EIN FROM mso.dbo.Events where Arbiter like '%Mr Lai%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 4766, EIN FROM mso.dbo.Events where Arbiter like '%Nathanael Lutton%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 8419, EIN FROM mso.dbo.Events where Arbiter like '%Nick Parish%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 4676, EIN FROM mso.dbo.Events where Arbiter like '%Peter Stouten%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 3037, EIN FROM mso.dbo.Events where Arbiter like '%Philip Nelkon%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 4676, EIN FROM mso.dbo.Events where Arbiter like '%Pieter Stouten%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 807, EIN FROM mso.dbo.Events where Arbiter like '%Roel Efting%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 10053, EIN FROM mso.dbo.Events where Arbiter like '%Roger Martin%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 8418, EIN FROM mso.dbo.Events where Arbiter like '%Stephen Agar%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 8416, EIN FROM mso.dbo.Events where Arbiter like '%Stephen Boniface%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 8416, EIN FROM mso.dbo.Events where Arbiter like '%Steve Boniface%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 4359, EIN FROM mso.dbo.Events where Arbiter like '%Steve Perry%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 4353, EIN FROM mso.dbo.Events where Arbiter like '%Paul Cartman%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 1424, EIN FROM mso.dbo.Events where Arbiter like '%Stewart Reuben%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 4251, EIN FROM mso.dbo.Events where Arbiter like '%Tony Atkins%'
INSERT INTO mso.dbo.Arbiters ([Arbiter Id], EIN) SELECT 8286, EIN FROM mso.dbo.Events where Arbiter like '%Tony Corfe%'

ALTER TABLE mso.dbo.Events DROP CONSTRAINT SSMA_CC$Events$Arbiter$disallow_zero_length
ALTER TABLE mso.dbo.Events DROP COLUMN Arbiter

-------------------------------------------------------
--- Blow away corrupt 2001-style Session data in Events
-------------------------------------------------------

ALTER TABLE mso.dbo.Events DROP CONSTRAINT SSMA_CC$Events$Sessions$disallow_zero_length
ALTER TABLE mso.dbo.Events DROP COLUMN Sessions

-------------------------------------------------------------
--- Make fully populated columns NOT NULL to make code nicer
-------------------------------------------------------------

ALTER TABLE mso.dbo.Names ALTER COLUMN Male BIT NOT NULL



