declare @original INT = 10902
declare @dupe INT = 11163

-- make sure fields carried across as appropriate
update mso_prod.dbo.Names set FirstName = 'Ricardo Jorge', [No News] = 1 where [mind sport id] = 10902

select * from mso_prod.dbo.Names where [Mind Sport Id] in (@dupe, @original)

select * from mso_prod.dbo.Arbiters where [Arbiter Id] in (@dupe, @original)

select * from mso_prod.dbo.Entrants where [Mind Sport Id] in (@dupe, @original)

select * from mso_prod.dbo.Payments where [MindSportsId] in (@dupe, @original)

select * from mso_prod.dbo.SelectedPeople where MSOID in (@dupe, @original)

update mso_prod.dbo.Arbiters set [Arbiter Id] = @original where [Arbiter Id] = @dupe

update mso_prod.dbo.Entrants set [Mind Sport id] = @original where [Mind Sport Id] = @dupe

update mso_prod.dbo.Payments set MindSportsId = @original where [MindSportsId] = @dupe

UPDATE mso_prod.dbo.SelectedPeople SET MSOID = @original where MSOID = @dupe

delete from mso_prod.dbo.Seedings where ContestantId = @dupe

delete from mso_prod.dbo.Names where [Mind Sport Id] = @dupe