USE mso

-------------------------------------------------
-- Various tables need an ID
-------------------------------------------------

ALTER TABLE [Olympiad Info] ADD Id INT IDENTITY(1,1)

ALTER TABLE [Olympiad Info] ADD PRIMARY KEY (Id)

ALTER TABLE Fees add Id INT IDENTITY(1,1)

ALTER TABLE Fees ADD PRIMARY KEY (Id)

ALTER TABLE Games add Id INT IDENTITY(1,1)

ALTER TABLE Games ADD PRIMARY KEY (Id)

ALTER TABLE [Payment Methods] ADD Id INT IDENTITY(1,1)

ALTER TABLE [Payment Methods] ADD PRIMARY KEY (Id)

ALTER TABLE [Entrants] add OlympiadId INT 

UPDATE e set OlympiadId = o.Id 
FROM entrants e
JOIN [Olympiad Info] o on e.year = o.yearof

ALTER TABLE Events add OlympiadId INT 

update e set OlympiadId = o.Id 
FROM events e
join [Olympiad Info] o on e.year = o.yearof

ALTER TABLE Events ADD FOREIGN KEY (OlympiadId) REFERENCES [Olympiad Info] (Id)

alter table Location add OlympiadId INT 
alter table Location add Id INT IDENTITY(1,1)
ALTER TABLE Location ADD PRIMARY KEY (Id)

update e set OlympiadId = o.Id 
FROM locaTION e
join [Olympiad Info] o on e.year = o.yearof

ALTER TABLE Location ADD FOREIGN KEY (OlympiadId) REFERENCES [Olympiad Info] (Id)



create table UserLogins
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

ALTER TABLE Arbiters ADD Id INT IDENTITY(1,1)

ALTER TABLE Arbiters ADD PRIMARY KEY (Id)

---------------------------------------------------------
--- Don't store any credit card data, even if out of date
---------------------------------------------------------

ALTER TABLE Payments
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

DROP INDEX [Payments$Card Post-code] ON Payments

ALTER TABLE Payments
DROP COLUMN [Card Name], [Expiry Date], [Issue], [Card number], [Card Address 1], [Card Address 2], [Card City], [Card County], [Card Post-code], [Card Country]


---------------------------------------------------------
--- Payments - turn Year into Olympiad Id
---------------------------------------------------------

alter table [Payments] add OlympiadId INT

update p set OlympiadId = o.Id 
FROM payments p
join [Olympiad Info] o on p.year = o.yearof

---------------------------------------------------------
--- Unify manually entered medals which are inconsistent
---------------------------------------------------------

UPDATE [Entrants] SET Medal = 'Gold JNR' WHERE Medal = 'Jnr Gold'

UPDATE [Entrants] SET Medal = 'Silver JNR' WHERE Medal = 'Jnr Silver'

UPDATE [Entrants] SET Medal = 'Bronze JNR' WHERE Medal = 'Jnr Bronze'

---------------------------------------------------------
--- Arbiters - remove redundant Events.Arbiter column 
--- 1: remove duplicate information
--- 2: copy across all existing (except for ?? and None)
--- 3: remove redundant column
---------------------------------------------------------

update events set arbiter = null where ein in (select ein from arbiters)

INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 4984, EIN FROM Events where Arbiter like '%Alan Farrell%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 4519, EIN FROM Events where Arbiter like '%Andrew Havery%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 10049, EIN FROM Events where Arbiter like '%Ann Chilcot%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 3470, EIN FROM Events where Arbiter like '%Arnold Lutton%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 610, EIN FROM Events where Arbiter like '%Aubrey de Grey%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 4289, EIN FROM Events where Arbiter like '%Barbara Carey%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 1282, EIN FROM Events where Arbiter like '%Barbara Corfe%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 9553, EIN FROM Events where Arbiter like '%Bernard Morgan%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 868, EIN FROM Events where Arbiter like '%Bill Hartston%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 8423, EIN FROM Events where Arbiter like '%Bob Wade%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 8421, EIN FROM Events where Arbiter like '%Bobby Raikhy%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 4377, EIN FROM Events where Arbiter like '%Chris Keeley%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 264, EIN FROM Events where Arbiter like '%CK Lai%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 8424, EIN FROM Events where Arbiter like '%Dan Glimne%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 663, EIN FROM Events where Arbiter like '%David Kotin%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 8415, EIN FROM Events where Arbiter like '%David Levy%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 8422, EIN FROM Events where Arbiter like '%David Lunn%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 10048, EIN FROM Events where Arbiter like '%David Norman%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 8401, EIN FROM Events where Arbiter like '%David Parlett%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 638, EIN FROM Events where Arbiter like '%David Sedgwick%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 8402, EIN FROM Events where Arbiter like '%Eric Johnson%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 5254, EIN FROM Events where Arbiter like '%Gerald Jacobs%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 183, EIN FROM Events where Arbiter like '%Marc Shaw%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 741, EIN FROM Events where Arbiter like '%Neville Belinfante%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 4254, EIN FROM Events where Arbiter like '%Glenda Trew%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 4251, EIN FROM Events where Arbiter like '%Go Arbiter%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 10050, EIN FROM Events where Arbiter like '%Ian Cawes%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 8373, EIN FROM Events where Arbiter like '%Jack Berkovi%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 8425, EIN FROM Events where Arbiter like '%Jim McCarthy%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 449, EIN FROM Events where Arbiter like '%Josef Kollar%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 10051, EIN FROM Events where Arbiter like '%Janet Green%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 8426, EIN FROM Events where Arbiter like '%Kris Burm%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 8414, EIN FROM Events where Arbiter like '%Lady Mary Tovey%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 4560, EIN FROM Events where Arbiter like '%Lester Luer%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 8413, EIN FROM Events where Arbiter like '%Malcolm%Carey%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 3252, EIN FROM Events where Arbiter like '%Mark Stretch%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 9125, EIN FROM Events where Arbiter like '%Martin Burroughs%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 8287, EIN FROM Events where Arbiter like '%Michael Crane%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 10089, EIN FROM Events where Arbiter like '%Michael Main%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 9493, EIN FROM Events where Arbiter like '%Mike Wellman%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 264, EIN FROM Events where Arbiter like '%Mr Lai%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 4766, EIN FROM Events where Arbiter like '%Nathanael Lutton%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 8419, EIN FROM Events where Arbiter like '%Nick Parish%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 4676, EIN FROM Events where Arbiter like '%Peter Stouten%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 3037, EIN FROM Events where Arbiter like '%Philip Nelkon%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 4676, EIN FROM Events where Arbiter like '%Pieter Stouten%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 807, EIN FROM Events where Arbiter like '%Roel Efting%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 10053, EIN FROM Events where Arbiter like '%Roger Martin%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 8418, EIN FROM Events where Arbiter like '%Stephen Agar%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 8416, EIN FROM Events where Arbiter like '%Stephen Boniface%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 8416, EIN FROM Events where Arbiter like '%Steve Boniface%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 4359, EIN FROM Events where Arbiter like '%Steve Perry%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 4353, EIN FROM Events where Arbiter like '%Paul Cartman%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 1424, EIN FROM Events where Arbiter like '%Stewart Reuben%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 4251, EIN FROM Events where Arbiter like '%Tony Atkins%'
INSERT INTO Arbiters ([Arbiter Id], EIN) SELECT 8286, EIN FROM Events where Arbiter like '%Tony Corfe%'

ALTER TABLE Events DROP CONSTRAINT SSMA_CC$Events$Arbiter$disallow_zero_length
ALTER TABLE Events DROP COLUMN Arbiter

-------------------------------------------------------
--- Blow away corrupt 2001-style Session data in Events
-------------------------------------------------------

ALTER TABLE Events DROP CONSTRAINT SSMA_CC$Events$Sessions$disallow_zero_length
ALTER TABLE Events DROP COLUMN Sessions

-------------------------------------------------------------
--- Make fully populated columns NOT NULL to make code nicer
-------------------------------------------------------------

ALTER TABLE Names ALTER COLUMN Male BIT NOT NULL

-------------------------------------------------------------
--- Event Number should be 0 not null
-------------------------------------------------------------

UPDATE Events SET Number = 0 WHERE Number IS NULL
DROP INDEX Events$Number ON Events
ALTER TABLE Events ALTER COLUMN Number INT NOT NULL

-------------------------------------------------------------
--- Fee should be 0 not null
-------------------------------------------------------------

UPDATE Entrants SET Fee = 0 WHERE Fee IS NULL
DROP INDEX Entrants$EntrantsFee ON Entrants
ALTER TABLE Entrants ALTER COLUMN Fee MONEY NOT NULL

-------------------------------------------------------------
--- Add GameId to Events
-------------------------------------------------------------

ALTER TABLE Events ADD GameId INT

UPDATE e SET GameId = g.Id
FROM Events e
JOIN Games g ON LEFT(e.Code, 2) = g.Code

ALTER TABLE Events ADD FOREIGN KEY (GameId) REFERENCES [Games] (Id)

------------------------------------------------------------
--- Add EventId to Entrant
------------------------------------------------------------

ALTER TABLE Entrants ADD EventId INT

UPDATE e SET EventId = ev.EIN
FROM Entrants e
JOIN Events ev on e.[Game Code] = ev.Code AND e.OlympiadId = ev.OlympiadId

ALTER TABLE Entrants ADD FOREIGN KEY (EventId) REFERENCES Events (EIN)

------------------------------------------------------------
--- Rework the Access types in Session
------------------------------------------------------------

ALTER TABLE Session ADD StartTime TIME, FinishTime TIME, IsActive BIT

UPDATE Session SET StartTime = CAST(Start AS TIME), FinishTime = CAST(Finish AS TIME), IsActive = 0
update session set IsActive = 1 where session in ('A10313','P2353','E63103','A10353')
update session set Worth = 1 where session = 'E63103'

--ALTER TABLE Session ALTER COLUMN StartTime TIME NOT NULL
--ALTER TABLE Session ALTER COLUMN FinishTime TIME NOT NULL
ALTER TABLE Session ALTER COLUMN IsActive BIT NOT NULL

ALTER TABLE Session DROP COLUMN Start, Finish

ALTER TABLE [Event Sess] ADD FOREIGN KEY (Session) REFERENCES [Session] (Session)

-------------------------------------------------------------
--- Make the OlympiadId not null in Events
-------------------------------------------------------------

ALTER TABLE Events ALTER COLUMN OlympiadId INT NOT NULL










