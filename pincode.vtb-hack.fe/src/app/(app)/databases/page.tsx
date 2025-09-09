"use client";

import React from "react";
import { Button } from "@pin-code/ui-kit";
import { Plus } from "lucide-react";
import { useCreateDatabaseModalStore } from "@store/database/create-modal";
import Image from "next/image";
import { useTheme } from "next-themes";

export default function DatabasesPage() {
	const { theme } = useTheme();
	const { open } = useCreateDatabaseModalStore();

	return (
		<div className="p-6 space-y-6 min-h-[100svh] flex flex-col">
			<div className="mb-6 flex justify-between items-start">
				<div>
					<h1 className="text-3xl font-bold">Базы данных</h1>
					<p className="text-muted-foreground mt-2">
						Подключите вашу БД для получения анализ и рекомендации по настройке кластера
					</p>
				</div>
			</div>

			<div className="flex flex-col items-center justify-between grow">
				{/*<div className="max-w-2xl mx-auto justify-center space-y-8">*/}

				<Image
					src={theme === "dark" ? "/images/database.png" : "/images/database-light.png"}
					alt={""}
					width={400}
					height={400}
				/>

				{/* Карточки с возможностями */}
				{/*<div className="grid md:grid-cols-2 gap-4 mt-8">*/}
				{/*	<Card className="text-left hover:shadow-lg transition-all duration-200 hover:scale-[1.02] cursor-pointer group">*/}
				{/*		<CardHeader>*/}
				{/*			<div className="w-10 h-10 bg-blue-100 dark:bg-blue-900/20 rounded-lg flex items-center justify-center mb-3 group-hover:scale-110 transition-transform">*/}
				{/*				<Database className="w-5 h-5 text-blue-600 dark:text-blue-400" />*/}
				{/*			</div>*/}
				{/*			<CardTitle className="text-lg">Выберите из списка</CardTitle>*/}
				{/*			<CardDescription>*/}
				{/*				Выберите уже подключенную базу данных из списка справа для просмотра анализа*/}
				{/*			</CardDescription>*/}
				{/*		</CardHeader>*/}
				{/*	</Card>*/}

				{/*	<Card*/}
				{/*		className="text-left hover:shadow-lg transition-all duration-200 hover:scale-[1.02] cursor-pointer group border-dashed border-2 hover:border-solid hover:border-blue-200 dark:hover:border-blue-800"*/}
				{/*		onClick={open}*/}
				{/*	>*/}
				{/*		<CardHeader>*/}
				{/*			<div className="w-10 h-10 bg-green-100 dark:bg-green-900/20 rounded-lg flex items-center justify-center mb-3 group-hover:scale-110 transition-transform">*/}
				{/*				<Plus className="w-5 h-5 text-green-600 dark:text-green-400" />*/}
				{/*			</div>*/}
				{/*			<CardTitle className="text-lg">Новое подключение</CardTitle>*/}
				{/*			<CardDescription>*/}
				{/*				Добавьте новую базу данных PostgreSQL для анализа производительности*/}
				{/*			</CardDescription>*/}
				{/*		</CardHeader>*/}
				{/*	</Card>*/}
				{/*</div>*/}

				<Button
					onClick={open}
					size="lg"
					className="bg-[#dd8a00] hover:bg-[#b57102] text-white px-8 py-3 rounded-xl shadow-lg hover:shadow-xl transition-all duration-200 transform hover:scale-102 cursor-pointer"
				>
					<Plus className="w-5 h-5 mr-2" />
					Подключить базу данных
				</Button>

				<div className="border-t border-border/40">
					<p className="text-sm text-muted-foreground">
						Поддерживаются PostgreSQL версии 15+, с установленным pg_stat_statements.
					</p>
				</div>
			</div>
		</div>
	);
}
