import numpy as np
import math

#reads file of sudoku puzzles and places them in arrays arranged by difficulty (file is specifically formatted prior to input)
def readPuzzleFile(file):
	easy = []
	medium = []
	hard = []
	evil = []

	difficulty = {
		"Easy": 1,
		"Medium": 2,
		"Hard": 3,
		"Evil": 4
	}


	#for each configuration in the text file:
	f = open(file, "r")
	file = f.read().splitlines()
	board = np.zeros((9,9))
	control = False
	i = 0

	for line in file:
		#determine which difficulty the puzzle is
		if len(line.split()) > 1:
			#place the board into the correct array
			if control == True:
				if difficulty[d] == 1:
					easy.append(board)
				elif difficulty[d] == 2:
					medium.append(board)
				elif difficulty[d] == 3:
					hard.append(board)
				elif difficulty[d] == 4:
					evil.append(board)
			control = True
			#throw out first string and read the second
			i = 0
			d = line.split()[1]
			board = np.zeros((9,9))
		#read the following nine lines in as a 9x9 array of ints (the sudoku board)
		else:
			for j in range(9):
				board[i][j] = int(line[j])
			i += 1
			if i == 9:
				i = 0
	return easy, medium, hard, evil

def alldiff(board, row, col):
	if board[row][col] != 0:
		return [board[row][col]]

	if row < 3:
		squareRow = 0
	elif row < 6:
		squareRow = 3
	else:
		squareRow = 6
	if col < 3:
		squareCol = 0
	elif col < 6:
		squareCol = 3
	else:
		squareCol = 6

	domain = [1,2,3,4,5,6,7,8,9]
	for i in range(9):
		if board[row][i] in domain and board[row][i] != 0 and i != col:
			domain.remove(board[row][i])
		if board[i][col] in domain and board[i][col] != 0 and i != row:
			domain.remove(board[i][col])
	for i in range(3):
		for j in range(3):
			if board[squareRow + i][squareCol + j] in domain and board[squareRow + i][squareCol + j] != 0:
				domain.remove(board[squareRow + i][squareCol + j])
	return domain


def success(assign):
	#check that board is filled
	for i in range(9):
		for j in range(9):
			if assign.board[i][j] == 0:
				return False
	
	for i in range(9):
		for j in range(9):
			if assign.domain[i][j] != [0]:
				return False
			
	return True

class assignment:
	domain = []
	board = []
	def __init__(self, board):
		self.board = board
		for i in range(9):
			self.domain.append([[],[],[],[],[],[],[],[],[]])
			for j in range(9):
				self.domain[i][j] = alldiff(board, i, j)
				if self.domain[i][j] == [0]:
					print("Test")

	def makeAssignment(self, row, col, assignment):
		self.board[row][col] = assignment
		self.domain[row][col] = [0]

	def assignmentComplete(self):
		for i in range(9):
			for j in range(9):
				if (self.domain[i][j]) != [0]:
					return False
		return True
		

	def nakedSingle(self):
		control = False
		for i in range(9):
			for j in range(9):
				if len(self.domain[i][j]) == 1 and self.domain[i][j][0] != 0:
					self.makeAssignment(i,j, self.domain[i][j][0])
					control = True
		return control

	def hiddenSingle(self):
		hidden = False
		
		for i in range(9):
			for j in range(9):
				#determine square
				if i < 3:
					squareRow = 0
				elif i < 6:
					squareRow = 3
				else:
					squareRow = 6
				if j < 3:
					squareCol = 0
				elif j < 6:
					squareCol = 3
				else:
					squareCol = 6

				control = False
				for d in self.domain[i][j]:
					controlRow = True
					controlCol = True
					controlBox = True
					for x in range(9):
						if control == False:
							if d in self.domain[i][x] and x != j:
								controlRow = False
							if d in self.domain[x][j] and x != i:
								controlCol = False
					for x in range(3):
						for y in range(3):
							if d in self.domain[squareCol + x][squareRow + y]:
								controlBox = False

					if controlRow and controlCol and controlBox:
						self.makeAssignment(i,j, d)
						hidden = True
		
		return hidden

	def pairs(self):
		b = False
		hidden = False
		for i in range(9):
			for j in range(9):
				#determine square
				if i < 3:
					squareRow = 0
				elif i < 6:
					squareRow = 3
				else:
					squareRow = 6
				if j < 3:
					squareCol = 0
				elif j < 6:
					squareCol = 3
				else:
					squareCol = 6

				for d in range(0, len(self.domain[i][j]) - 1):
					control = False
					for z in range(d + 1, len(self.domain[i][j])):
						common = []
					
						count = 0
						if control == False:
							for x in range(j - 1,9):
								if self.domain[i][j][d] in self.domain[i][x] and self.domain[i][j][z] in self.domain[i][x] and x != j:
									count += 1
								if self.domain[i][j][d] in self.domain[x][j] and self.domain[i][j][z] in self.domain[x][j] and x != i:
									count += 1
							for x in range(3):
								for y in range(3):
									if len(self.domain) > 2 and self.domain[i][j][d] in self.domain[x + squareRow][y + squareCol] and self.domain[i][j][z] in self.domain[x + squareRow][y + squareCol] and (x != i and y != j):
										count += 1

							if count < 2:
								common.append([i, x, self.domain[i][j][d], self.domain[i][j][z]])
							else:
								common = []
								control = False
							if count == 1:
								control = True

							count = 0
							if control:
								if len(self.domain[i][j]) > 2 or len(self.domain[common[0][0]][common[0][1]]) > 2:
									hidden = True
								self.domain[i][j] = [self.domain[i][j][d], self.domain[i][j][z]]
								if self.domain[common[0][0]][common[0][1]] != [0]:
									self.domain[common[0][0]][common[0][1]] = [self.domain[i][j][0], self.domain[i][j][1]]
							common = []
		return hidden

	def triples(self):
		hidden = False
		for i in range(9):
			for j in range(9):
				#determine square
				if i < 3:
					squareRow = 0
				elif i < 6:
					squareRow = 3
				else:
					squareRow = 6
				if j < 3:
					squareCol = 0
				elif j < 6:
					squareCol = 3
				else:
					squareCol = 6

				for d in range(0, len(self.domain[i][j]) - 2):
					control = False
					for z in range(d + 1, len(self.domain[i][j]) - 1):
						for w in range(z + 1, len(self.domain[i][j])):
								common = []

								count = 0
								if control == False:
									for x in range(9):
										if self.domain[i][j][d] in self.domain[i][x] and self.domain[i][j][z] in self.domain[i][x] and self.domain[i][j][w] in self.domain[i][x] and x != j:
											count += 1
										if self.domain[i][j][d] in self.domain[x][j] and self.domain[i][j][z] in self.domain[x][j] and self.domain[i][j][w] in self.domain[x][j] and x != i:
											count += 1
									for x in range(3):
										for y in range(3):
											if len(self.domain) > 3 and self.domain[i][j][d] in self.domain[x + squareRow][y + squareCol] and self.domain[i][j][z] in self.domain[x + squareRow][y + squareCol] and self.domain[i][j][w] in self.domain[x + squareRow][y + squareCol] and (x != i and y != j):
												count += 1

									if count < 3:
										common.append([i, x, self.domain[i][j][d], self.domain[i][j][z], self.domain[i][j][w]])
									else:
										common = []
										control = False
									if count == 2:
										control = True

									count = 0
									if control:
										if len(self.domain[i][j]) > 3 or len(self.domain[common[0][0]][common[0][1]]) > 3:
											hidden = True
										self.domain[i][j] = [self.domain[i][j][d], self.domain[i][j][z], self.domain[i][j][w]]
										self.domain[common[0][0]][common[0][1]] = [self.domain[i][j][0], self.domain[i][j][1], self.domain[i][j][2]]
									common = []
		return hidden

	def inference(self, level):
		control = False
		while control == False:
			nakedS = self.nakedSingle()
			hiddenS = self.hiddenSingle()
			if hiddenS == False:
				if level > 1:
					pair = self.pairs()
				else:
					pair = False
				if pair == False:
					if level > 2:
						tri = self.triples()
					else:
						tri = False
					if tri == False:
						control = True



def backtrackSearch(assign, inferenceLvl, bound, backtrack, depth, MCV = False):
	if success(assign) or depth == bound:
		return assign, backtrack, depth + 1
		
	for i in range(9):
		for j in range(9):
			alldiff(assign.board, i, j)
			if len(assign.domain[i][j]) == 0:
				return assignment(np.zeros((9,9))), backtrack + 1, depth
	if inferenceLvl > 0:
		assign.inference(inferenceLvl)
		
	for i in range(9):
		for j in range(9):
			if len(assign.domain[i][j]) == 1 and assign.domain[i][j][0] != 0:
				assign.makeAssignment(i,j,assign.domain[i][j][0])
	
	varRow = 0
	varCol = 0	
	if MCV:
		minRow = 0
		minCol = 0
		minDomain = math.inf
		for i in range(9):
			for j in range(9):
				if len(assign.domain[i][j]) < minDomain and assign.domain[i][j] != 0:
					minDomain = len(assign.domain[i][j])
					minRow = i
					minCol = j
		varRow = minRow
		varCol = minCol
	else:
		control = False
		while control == False:
			if assign.domain[varRow][varCol] != [] and assign.domain[varRow][varCol][0] != 0:
				control = True
			else:
				varCol += 1
				if varCol >= 9:
					varRow += 1
					varCol = 0
				if varRow == 9:
					return assignment(np.zeros((9,9))), backtrack + 1, depth
		
	
	for i in assign.domain[varRow][varCol]:
		tempAssign = assign
		tempAssign.makeAssignment(varRow,varCol, i)
		assign, backtrack, depth = backtrackSearch(tempAssign, inferenceLvl, bound, backtrack + 1, depth + 1, MCV)
		if assign != []:
			return tempAssign, backtrack, depth
	return assignment(np.zeros((9,9))), backtrack + 1, depth

f = open("zeros.csv", "w")
easy, medium, hard, evil = readPuzzleFile("sudoku-problems.txt")
f.write("Easy\n")
for i in easy:
	count = 0
	for j in i:
		for k in j:
			if k == [0]:
				count += 1
	f.write(str(count) + "\n")
f.write("medium\n")
for i in medium:
	count = 0
	for j in i:
		for k in j:
			if k == [0]:
				count += 1
	f.write(str(count) + "\n")
f.write("hard\n")
for i in hard:
	count = 0
	for j in i:
		for k in j:
			if k == [0]:
				count += 1
	f.write(str(count) + "\n")
f.write("evil\n")
for i in evil:
	count = 0
	for j in i:
		for k in j:
			if k == [0]:
				count += 1
	f.write(str(count) + "\n")
	
f.close()
'''
assign = assignment(easy[0])
s, backtracks, depth = backtrackSearch(assign, 3, 950, 0, 0)
for j in range(3):
	f.write("Easy, Constant Search\nLevel of inference, # of backtracks, depth, # of empty spaces\n")
	for i in easy:
		assign = assignment(i)
		s, backtracks, depth = backtrackSearch(assign, j, 950, 0, 0)
		count = 0
		for x in i:
			for y in x:	
				if y == 0:
					count += 1
		f.write(str(j) + "," + str(backtracks) + "," + str(depth) + str(count) + "\n")
	f.write("Easy, MCV\nLevel of inference, # of backtracks, depth, # of empty spaces\n")
	for i in easy:
		assign = assignment(i)
		s, backtracks, depth = backtrackSearch(assign, j, 950, 0, 0, True)
		count = 0
		for x in i:
			for y in x:	
				count += 1
		f.write(str(j) + "," + str(backtracks) + "," + str(depth) + str(count) + "\n")
	f.write("Medium, Constant Search\nLevel of inference, # of backtracks, depth, # of empty spaces\n")
	for i in medium:
		assign = assignment(i)
		s, backtracks, depth = backtrackSearch(assign, j, 950, 0, 0)
		count = 0
		for x in i:
			for y in x:	
				count += 1
		f.write(str(j) + "," + str(backtracks) + "," + str(depth) + str(count) + "\n")
	f.write("Medium, MCV\nLevel of inference, # of backtracks, depth, # of empty spaces\n")
	for i in medium:
		assign = assignment(i)
		s, backtracks, depth = backtrackSearch(assign, j, 950, 0, 0, True)
		count = 0
		for x in i:
			for y in x:	
				count += 1
		f.write(str(j) + "," + str(backtracks) + "," + str(depth) + str(count) + "\n")
	f.write("Hard, Constant Search\nLevel of inference, # of backtracks, depth, # of empty spaces\n")
	for i in hard:
		assign = assignment(i)
		s, backtracks, depth = backtrackSearch(assign, j, 950, 0, 0)
		count = 0
		for x in i:
			for y in x:	
				count += 1
		f.write(str(j) + "," + str(backtracks) + "," + str(depth) + str(count) + "\n")
	f.write("Hard, MCV\nLevel of inference, # of backtracks, depth, # of empty spaces\n")
	for i in hard:
		assign = assignment(i)
		s, backtracks, depth = backtrackSearch(assign, j, 950, 0, 0, True)
		count = 0
		for x in i:
			for y in x:	
				count += 1
		f.write(str(j) + "," + str(backtracks) + "," + str(depth) + str(count) + "\n")
	f.write("Evil, Constant Search\nLevel of inference, # of backtracks, depth, # of empty spaces\n")
	for i in evil:
		assign = assignment(i)
		s, backtracks, depth = backtrackSearch(assign, j, 950, 0, 0)
		count = 0
		for x in i:
			for y in x:	
				count += 1
		f.write(str(j) + "," + str(backtracks) + "," + str(depth) + str(count) + "\n")
	f.write("Evil, MCV\nLevel of inference, # of backtracks, depth, # of empty spaces\n")
	for i in evil:
		assign = assignment(i)
		s, backtracks, depth = backtrackSearch(assign, j, 950, 0, 0, True)
		count = 0
		for x in i:
			for y in x:	
				count += 1
		f.write(str(j) + "," + str(backtracks) + "," + str(depth) + str(count) + "\n")
f.close()'''