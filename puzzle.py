#This program implements the RDFS and IDA* to solve a 15 puzzle
import random
import numpy as np

class game:
	position = [3,3]
	board = np.zeros((4,4))
	compass = ["up","down","left","right"]
	direction = {
		"up": (-1, 0),
		"down": (1, 0),
		"right": (0, 1),
		"left": (0, -1)
	}

	#create a 4x4 grid with values 0 to 15 randomly assigned (0 is considered the empty space)
	def __init__(self):
		count = 1
		for i in range(4):
			for j in range(4):
				self.board[i][j] = count
				count += 1
		self.board[3][3] = 0

	#returns a board position for a given a move
	def move(self, direction):
		board = np.copy(self.board)
		position = np.copy(self.position)

		#check for illegal moves
		if position[0] == 0 and direction == "up":
			return []
		if position[0] == 3 and direction == "down":
			return []
		if position[1] == 0 and direction == "left":
			return []
		if position[1] == 3 and direction == "right":
			return []

		move = [position[0] + self.direction[direction][0], position[1] + self.direction[direction][1]]
		board[position[0]][position[1]] = board[move[0]][move[1]]
		board[move[0]][move[1]] = 0
		return board, move

	#executes a move on the board
	def performMove(self, direction):
		if self.move(direction) != []:
			self.board, self.position = self.move(direction)
			#print(self.board)
			return True
		else:
			return False

	#checks for sucessful board state
	def goalTest(self):
		count = 1
		for i in range(4):
			for j in range(4):
				if i == 3 and j == 3:
					return True
				elif self.board[i][j] != count:
					return False
				count += 1

	#Performs a series of m random moves where m is an inputted integer value
	def scramble(self, m):
		compass = ["up", "down", "left", "right"]
		for i in range(m):
			control = False
			while control == False:
				if self.performMove(compass[random.randint(0,3)]):
					control = True

#measures a manhattan heuristic	of a given board position
def manhattan(board):
	h = 0
	for i in range(15):
		col = 0
		row = 0
		loc = np.where(board == i + 1)
		for j in range(i):
			col += 1
			if col == 4:
				col = 0
				row += 1
		h += abs(loc[0] - row) + abs(loc[1] - col)
	return h

def IDA(game, path = [], g = 0):
	if game.goalTest():
		return path, len(path)
	choice = []
	for i in range(4):
		if game.move(game.compass[i]) != []:
			choice.append((game.move(game.compass[i]), game.compass[i])
	minCost = choice[0]
	for i in range(1,len(choice)):
		if manhattan(i) < manhattan(minCost):
			minCost = choice[i]
	if manhattan(minCost) + 1 + g < 



g1 = game()

g1.scramble(10)
IDA(g1)
