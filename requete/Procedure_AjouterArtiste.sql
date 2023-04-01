USE Lab09_Employes
GO
EXEC Employes.USP_AjouterArtiste 
@Prenom = 'Bruno',
@Nom = 'Pouiot', 
@NoTel = '4025468992',
@Courriel = 'Bruno@example.com', 
@Specialite = 'bof'